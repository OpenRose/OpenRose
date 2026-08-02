/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/


// Touch-only swipe detection (single-finger). Does NOT handle mouse/pointer events.
// - Intercepts single-finger horizontal swipes and prevents browser history only for that gesture.
// - Immediately cancels custom handling when a second finger is detected, allowing pinch/zoom.
// - Moderate sensitivity: threshold=50px, restraint=120px, allowedTime=600ms.
// - Set debug = true for logging during testing.

(function () {
    if (window.openRoseSwipeLibLoaded) return;
    window.openRoseSwipeLibLoaded = true;
    window.openRoseHandlers = window.openRoseHandlers || {};

    var debug = false; // set to true while testing to get console logs

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
            try {
                if (savedOverscroll !== null) {
                    document.documentElement.style.overscrollBehavior = savedOverscroll;
                    savedOverscroll = null;
                } else {
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
                // Only begin tracking when exactly one touch is present
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

                dlog("swipe: touchstart id", activeTouch.id, "element", elementKey);

                // Temporarily reduce overscroll behavior so browsers are less likely to interpret horizontal swipe as nav.
                try {
                    savedOverscroll = document.documentElement.style.overscrollBehavior || '';
                    document.documentElement.style.overscrollBehavior = 'none';
                } catch (e) { savedOverscroll = null; }

                try {
                    savedTouchAction = el.style.touchAction || '';
                    el.style.touchAction = 'pan-y';
                } catch (e) { savedTouchAction = null; }

                // Install document-level touchmove (non-passive) so we can preventDefault when we detect horizontal gesture
                docTouchMoveHandler = function (moveEv) {
                    try {
                        if (!activeTouch) return;

                        // If multi-touch occurs, cancel custom handling immediately and restore defaults
                        if (moveEv.touches && moveEv.touches.length > 1) {
                            dlog("swipe: multi-touch detected -> cancel custom swipe handling", elementKey);
                            cleanupMoveHandler();
                            activeTouch = null;
                            return;
                        }

                        // find tracked touch
                        var found = null;
                        for (var i = 0; i < moveEv.touches.length; i++) {
                            if (moveEv.touches[i].identifier === activeTouch.id) { found = moveEv.touches[i]; break; }
                        }
                        if (!found) return;

                        var dx = found.clientX - activeTouch.startX;
                        var dy = found.clientY - activeTouch.startY;
                        var absDx = Math.abs(dx), absDy = Math.abs(dy);

                        // If horizontal dominant and passes a small early threshold, prevent default to stop browser nav
                        if (!activeTouch.prevented && absDx > EARLY_MOVE_THRESHOLD && absDx > absDy) {
                            try {
                                moveEv.preventDefault();
                                activeTouch.prevented = true;
                                dlog("swipe: prevented default on touchmove (element)", elementKey);
                            } catch (e) { /* ignore */ }
                        } else if (activeTouch.prevented) {
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

                if (!ev.changedTouches) {
                    cleanupMoveHandler();
                    activeTouch = null;
                    return;
                }

                var matched = null;
                for (var i = 0; i < ev.changedTouches.length; i++) {
                    if (ev.changedTouches[i].identifier === activeTouch.id) { matched = ev.changedTouches[i]; break; }
                }

                if (!matched) {
                    // touch ended but not the tracked one -> cleanup and return
                    cleanupMoveHandler();
                    activeTouch = null;
                    return;
                }

                var dx = matched.clientX - activeTouch.startX;
                var dy = matched.clientY - activeTouch.startY;
                var dt = Date.now() - activeTouch.startTime;

                dlog("swipe: touchend id", activeTouch.id, "dx", dx, "dy", dy, "dt", dt, "prevented", activeTouch.prevented);

                cleanupMoveHandler();

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
                dlog("swipe: touchcancel for", elementKey);
                cleanupMoveHandler();
                activeTouch = null;
            } catch (e) { /* ignore */ }
        }

        // Attach element-level touch listeners. Do NOT attach pointer/mouse handlers.
        try {
            el.addEventListener('touchstart', onTouchStart, { passive: false });
            el.addEventListener('touchend', onTouchEnd, { passive: true });
            el.addEventListener('touchcancel', onTouchCancel, { passive: true });
            dlog("swipe: attached touch handlers for element", elementKey);
        } catch (e) {
            // fallback
            el.addEventListener('touchstart', onTouchStart);
            el.addEventListener('touchend', onTouchEnd);
            el.addEventListener('touchcancel', onTouchCancel);
        }

        window.openRoseHandlers[elementKey] = { el: el, handlers: { onTouchStart: onTouchStart, onTouchEnd: onTouchEnd, onTouchCancel: onTouchCancel }, dotNetRef: dotNetRef };
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
                el.removeEventListener('touchstart', h.onTouchStart, { passive: false });
                el.removeEventListener('touchend', h.onTouchEnd, { passive: true });
                el.removeEventListener('touchcancel', h.onTouchCancel, { passive: true });
            } catch (e) {
                try { el.removeEventListener('touchstart', h.onTouchStart); el.removeEventListener('touchend', h.onTouchEnd); el.removeEventListener('touchcancel', h.onTouchCancel); } catch (_) { }
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

    // API for Blazor
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