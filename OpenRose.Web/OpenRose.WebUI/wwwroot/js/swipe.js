/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

// Touch-only swipe detection (single-finger). Does NOT handle mouse/pointer events.
// - Preserves mouse clicks completely (no mouse handlers attached).
// - Preserves pinch/zoom (multi-touch ignored).
// - Dynamically prevents browser history navigation for one-finger horizontal gestures.
// - Moderate sensitivity: threshold=50px, restraint=120px, allowedTime=600ms.
// - Set debug = true for logging during testing.

(function () {
    if (window.openRoseSwipeLibLoaded) return;
    window.openRoseSwipeLibLoaded = true;
    window.openRoseHandlers = window.openRoseHandlers || {};

    var debug = true; // set to true for device logs

    function dlog() {
        if (!debug) return;
        try { console.log.apply(console, arguments); } catch (_) { }
    }

    var DEFAULT_THRESHOLD = 50;   // px horizontal
    var DEFAULT_RESTRAINT = 120;  // px vertical
    var DEFAULT_ALLOWED_TIME = 600; // ms
    var EARLY_MOVE_THRESHOLD = 10; // px; start preventing after small horizontal movement

    function registerOnElement(el, dotNetRef, elementKey, opts) {
        if (!el) return false;

        var threshold = (opts && opts.threshold) || DEFAULT_THRESHOLD;
        var restraint = (opts && opts.restraint) || DEFAULT_RESTRAINT;
        var allowedTime = (opts && opts.allowedTime) || DEFAULT_ALLOWED_TIME;

        var activeTouch = null; // { id, startX, startY, startTime, prevented }
        var docTouchMoveHandler = null;
        var savedOverscroll = null;
        var savedTouchAction = null;

        function cleanupMoveHandler() {
            if (docTouchMoveHandler) {
                try { document.removeEventListener('touchmove', docTouchMoveHandler, { capture: true }); } catch (e) { /* ignore */ }
                docTouchMoveHandler = null;
            }
            // restore overscroll behavior & touch-action
            try {
                if (savedOverscroll !== null) {
                    document.documentElement.style.overscrollBehavior = savedOverscroll;
                    savedOverscroll = null;
                } else {
                    // if there was no saved value, remove explicit style (let CSS decide)
                    document.documentElement.style.overscrollBehavior = '';
                }
            } catch (e) { /* ignore */ }

            try {
                if (savedTouchAction !== null) {
                    el.style.touchAction = savedTouchAction;
                    savedTouchAction = null;
                } else {
                    el.style.touchAction = '';
                }
            } catch (e) { /* ignore */ }
        }

        function onTouchStart(ev) {
            try {
                if (!ev || !ev.touches) return;
                // Only one-finger gestures: if more than 1, ignore to preserve pinch
                if (ev.touches.length !== 1) {
                    activeTouch = null;
                    return;
                }

                var t = ev.touches[0];
                activeTouch = {
                    id: t.identifier,
                    startX: t.clientX,
                    startY: t.clientY,
                    startTime: Date.now(),
                    prevented: false
                };

                dlog("swipe: touchstart registered id", activeTouch.id, "element", elementKey);

                // Save current overscroll & touch-action and set temporary values that help block nav on Chromium
                try {
                    // Save and then set overscroll to 'none' to block "swipe to navigate" on Chromium browsers
                    savedOverscroll = document.documentElement.style.overscrollBehavior || '';
                    document.documentElement.style.overscrollBehavior = 'none';
                } catch (e) { savedOverscroll = null; }

                try {
                    savedTouchAction = el.style.touchAction || '';
                    // Hint: allow vertical panning but make horizontal available for JS handling
                    el.style.touchAction = 'pan-y';
                } catch (e) { savedTouchAction = null; }

                // Install document-level touchmove with passive: false so we can preventDefault when we detect horizontal gesture
                docTouchMoveHandler = function (moveEv) {
                    try {
                        if (!activeTouch) return;

                        // find the tracked touch in the current touches
                        var found = null;
                        for (var i = 0; i < moveEv.touches.length; i++) {
                            if (moveEv.touches[i].identifier === activeTouch.id) {
                                found = moveEv.touches[i];
                                break;
                            }
                        }
                        if (!found) return;

                        // If more than one touch currently active, abort preventing (user used another finger)
                        if (moveEv.touches.length > 1) return;

                        var dx = found.clientX - activeTouch.startX;
                        var dy = found.clientY - activeTouch.startY;
                        var absDx = Math.abs(dx), absDy = Math.abs(dy);

                        // If horizontal movement is dominant and exceeds early threshold, prevent default to stop browser nav
                        if (!activeTouch.prevented && absDx > EARLY_MOVE_THRESHOLD && absDx > absDy) {
                            try {
                                moveEv.preventDefault(); // this prevents browser swipe-to-navigate on many browsers
                                activeTouch.prevented = true;
                                dlog("swipe: prevented default on touchmove (element)", elementKey);
                            } catch (e) {
                                // some browsers restrict preventDefault if listener is passive (we used non-passive) or other constraints
                            }
                        } else if (activeTouch.prevented) {
                            // keep preventing if we've started preventing
                            try { moveEv.preventDefault(); } catch (e) { /* ignore */ }
                        }
                    } catch (e) { /* ignore */ }
                };

                document.addEventListener('touchmove', docTouchMoveHandler, { passive: false, capture: true });

            } catch (err) {
                dlog("swipe: onTouchStart error", err);
            }
        }

        function onTouchEnd(ev) {
            try {
                if (!activeTouch) {
                    cleanupMoveHandler();
                    return;
                }

                // find changedTouches that match the tracked touch id
                if (!ev.changedTouches) {
                    cleanupMoveHandler();
                    activeTouch = null;
                    return;
                }

                var matched = null;
                for (var i = 0; i < ev.changedTouches.length; i++) {
                    if (ev.changedTouches[i].identifier === activeTouch.id) {
                        matched = ev.changedTouches[i];
                        break;
                    }
                }

                if (!matched) {
                    // Might have ended elsewhere; just cleanup
                    cleanupMoveHandler();
                    activeTouch = null;
                    return;
                }

                var dx = matched.clientX - activeTouch.startX;
                var dy = matched.clientY - activeTouch.startY;
                var dt = Date.now() - activeTouch.startTime;

                dlog("swipe: touchend id", activeTouch.id, "dx", dx, "dy", dy, "dt", dt, "prevented", activeTouch.prevented);

                // cleanup move handler and styles
                cleanupMoveHandler();

                // evaluate threshold
                if (dt <= allowedTime && Math.abs(dx) >= threshold && Math.abs(dy) <= restraint) {
                    if (dx < 0) {
                        dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /*ignore*/ });
                        dlog("swipe: invoked left for", elementKey);
                    } else {
                        dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /*ignore*/ });
                        dlog("swipe: invoked right for", elementKey);
                    }
                }

                activeTouch = null;
            } catch (err) {
                dlog("swipe: onTouchEnd error", err);
                cleanupMoveHandler();
                activeTouch = null;
            }
        }

        function onTouchCancel(ev) {
            try {
                cleanupMoveHandler();
                activeTouch = null;
                dlog("swipe: touchcancel for", elementKey);
            } catch (e) { /* ignore */ }
        }

        // Attach element-level touch listeners. We don't add pointer or mouse listeners at all.
        var handlers = {
            touchstart: onTouchStart,
            touchend: onTouchEnd,
            touchcancel: onTouchCancel
        };

        try {
            // attach touchstart non-passive? touchstart cannot be passive (spec default is non-passive). But to be safe we attach with passive:false so that preventDefault in move works reliably later.
            el.addEventListener('touchstart', handlers.touchstart, { passive: false });
            el.addEventListener('touchend', handlers.touchend, { passive: true });
            el.addEventListener('touchcancel', handlers.touchcancel, { passive: true });
            dlog("swipe: attached touch handlers for element", elementKey);
        } catch (e) {
            // older browsers fallback
            el.addEventListener('touchstart', handlers.touchstart);
            el.addEventListener('touchend', handlers.touchend);
            el.addEventListener('touchcancel', handlers.touchcancel);
        }

        // Store registered handlers so we can unregister later
        window.openRoseHandlers[elementKey] = {
            el: el,
            handlers: handlers,
            dotNetRef: dotNetRef
        };

        dlog("swipe: registered touch-only element", elementKey);
        return true;
    }

    function unregisterElement(element) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : null;
            if (!elementKey) return false;
            var rec = window.openRoseHandlers[elementKey];
            if (!rec) return false;
            var el = rec.el;
            var h = rec.handlers || {};
            try {
                el.removeEventListener('touchstart', h.touchstart, { passive: false });
                el.removeEventListener('touchend', h.touchend, { passive: true });
                el.removeEventListener('touchcancel', h.touchcancel, { passive: true });
            } catch (e) {
                try { el.removeEventListener('touchstart', h.touchstart); el.removeEventListener('touchend', h.touchend); el.removeEventListener('touchcancel', h.touchcancel); } catch (_) { }
            }
            delete window.openRoseHandlers[elementKey];
            try { delete element.dataset.openroseId; } catch (_) { }
            dlog("swipe: unregistered element", elementKey);
            return true;
        } catch (err) {
            dlog("swipe: unregister error", err);
            return false;
        }
    }

    // Exposed API used by Blazor
    window.openRoseRegisterSwipeElement = function (element, dotNetRef, options) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : ("el-" + Math.random().toString(36).substr(2, 9));
            try { element.dataset.openroseId = elementKey; } catch (e) { /* ignore */ }
            var ok = registerOnElement(element, dotNetRef, elementKey, options || {});
            dlog("swipe: register result for", elementKey, ok);
            return ok;
        } catch (err) {
            dlog("swipe: register error", err);
            return false;
        }
    };

    window.openRoseUnregisterSwipeElement = function (element) {
        try {
            return unregisterElement(element);
        } catch (err) {
            dlog("swipe: unregister error", err);
            return false;
        }
    };
})();