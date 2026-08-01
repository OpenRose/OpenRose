/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

// wwwroot/js/swipe.js
// Hybrid element-level start + dynamic move-handler approach.
// - Prevents browser history swipe for single-finger horizontal swipes by calling preventDefault on move,
//   but only after detecting a single-finger horizontal gesture in progress.
// - Preserves pinch/zoom (multi-touch) and normal clicks (no pointer capture).
// - Moderate sensitivity: threshold=50px, restraint=120px, allowedTime=600ms.
// - Set debug = true for console logs during testing.

(function () {
    if (window.openRoseSwipeLibLoaded) return;
    window.openRoseSwipeLibLoaded = true;
    window.openRoseHandlers = window.openRoseHandlers || {};

    var debug = true; // set true while testing to get console logs

    function dlog() {
        if (!debug) return;
        console.log.apply(console, arguments);
    }

    // Sensitivity defaults (tweak if needed)
    var DEFAULT_THRESHOLD = 50;   // px horizontal
    var DEFAULT_RESTRAINT = 120;  // px vertical
    var DEFAULT_ALLOWED_TIME = 600; // ms

    // Helper: attach and remove with stored refs
    function registerOnElement(el, dotNetRef, elementKey, opts) {
        if (!el) return false;

        var threshold = (opts && opts.threshold) || DEFAULT_THRESHOLD;
        var restraint = (opts && opts.restraint) || DEFAULT_RESTRAINT;
        var allowedTime = (opts && opts.allowedTime) || DEFAULT_ALLOWED_TIME;

        // Per-interaction state
        // For pointer events: keep map from pointerId to meta { startX, startY, startTime, movedHorizontally }
        var pointerMeta = {};

        // For touch fallback (touch identifiers)
        // Map touchId -> meta
        var touchMeta = {};

        // Move handler refs so we can remove them
        var currentPointerMoveHandler = null;
        var currentTouchMoveHandler = null;

        // --- ELEMENT-LEVEL START HANDLERS ---

        function onElementPointerDown(ev) {
            // Only care about primary mouse button or touch
            if (ev.pointerType === 'mouse' && ev.button !== 0) return;

            // Record start for this pointer
            pointerMeta[ev.pointerId] = {
                startX: ev.clientX,
                startY: ev.clientY,
                startTime: Date.now(),
                prevented: false // whether we've prevented default (to block browser gestures)
            };

            // Install a pointermove handler on document so we can call preventDefault when necessary.
            // Use passive: false so preventDefault works.
            currentPointerMoveHandler = function (moveEv) {
                try {
                    // Only handle same pointerId
                    var meta = pointerMeta[ev.pointerId];
                    if (!meta) return;

                    // If pointer is not touch, don't call preventDefault (mouse dragging should not be blocked)
                    if (ev.pointerType !== 'touch') return;

                    var dx = moveEv.clientX - meta.startX;
                    var dy = moveEv.clientY - meta.startY;
                    var absDx = Math.abs(dx), absDy = Math.abs(dy);

                    // If multi-pointer active (additional pointers), abort preventing (preserve pinch)
                    // We can detect additional pointers via navigator.maxTouchPoints? Not reliable; check pointer events currently active:
                    // If any other pointer id exists in pointerMeta besides ev.pointerId, treat as multi-touch and skip.
                    var multi = false;
                    for (var pid in pointerMeta) {
                        if (pid !== String(ev.pointerId)) { multi = true; break; }
                    }
                    if (multi) return;

                    // If horizontal dominant and passes small movement threshold (not final swipe threshold)
                    var earlyThreshold = 10; // small: start preventing only if user moves noticeably horizontally
                    if (!meta.prevented && absDx > earlyThreshold && absDx > absDy) {
                        // prevent browser horizontal gestures (history) for this pointer
                        try {
                            moveEv.preventDefault();
                            meta.prevented = true;
                            dlog("swipe: prevented default on pointermove (element)", elementKey);
                        } catch (e) {
                            // Some browsers ignore preventDefault if passive true on some targets.
                        }
                    } else if (meta.prevented) {
                        // already prevented once, keep preventing
                        try { moveEv.preventDefault(); } catch (e) { }
                    }
                } catch (e) { /* ignore */ }
            };

            // attach to document
            document.addEventListener('pointermove', currentPointerMoveHandler, { passive: false, capture: true });

            dlog("swipe: pointerdown registered", ev.pointerId, elementKey);
        }

        function onElementPointerUp(ev) {
            // compute and clean up
            try {
                var meta = pointerMeta[ev.pointerId];
                // remove move handler
                if (currentPointerMoveHandler) {
                    document.removeEventListener('pointermove', currentPointerMoveHandler, { capture: true });
                    currentPointerMoveHandler = null;
                }

                if (!meta) return;

                var dx = ev.clientX - meta.startX;
                var dy = ev.clientY - meta.startY;
                var dt = Date.now() - meta.startTime;

                dlog("swipe: pointerup", ev.pointerId, "dx", dx, "dy", dy, "dt", dt, "meta.prevented", meta.prevented);

                // cleanup meta
                delete pointerMeta[ev.pointerId];

                if (dt <= allowedTime && Math.abs(dx) >= threshold && Math.abs(dy) <= restraint) {
                    if (dx < 0) {
                        dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /* ignore */ });
                        dlog("swipe: invoke left", elementKey);
                    } else {
                        dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /* ignore */ });
                        dlog("swipe: invoke right", elementKey);
                    }
                }
            } catch (e) { dlog("swipe: elementPointerUp error", e); }
        }

        function onElementPointerCancel(ev) {
            try {
                if (currentPointerMoveHandler) {
                    document.removeEventListener('pointermove', currentPointerMoveHandler, { capture: true });
                    currentPointerMoveHandler = null;
                }
                delete pointerMeta[ev.pointerId];
                dlog("swipe: pointercancel cleaned", ev.pointerId);
            } catch (e) { /* ignore */ }
        }

        // --- TOUCH fallback ---

        function onElementTouchStart(ev) {
            try {
                if (!ev || !ev.touches) return;
                // If a multi-touch begins, ignore swipe tracking for those touches (preserve pinch)
                if (ev.touches.length !== 1) {
                    // mark nothing
                    return;
                }
                var t = ev.touches[0];
                touchMeta[t.identifier] = {
                    startX: t.clientX,
                    startY: t.clientY,
                    startTime: Date.now(),
                    prevented: false
                };

                // install touchmove on document (non-passive) to be able to prevent default if horizontal gesture detected
                currentTouchMoveHandler = function (moveEv) {
                    try {
                        if (!moveEv || !moveEv.touches) return;
                        // find the touch with same identifier if present in touches or changedTouches
                        var found = null;
                        for (var i = 0; i < moveEv.touches.length; i++) {
                            if (touchMeta[moveEv.touches[i].identifier]) { found = moveEv.touches[i]; break; }
                        }
                        // If multiple touches present => abort preventing
                        if (moveEv.touches.length > 1) return;

                        // If found our tracked touch
                        if (!found) {
                            // maybe it's in changedTouches
                            for (var j = 0; j < moveEv.changedTouches.length; j++) {
                                if (touchMeta[moveEv.changedTouches[j].identifier]) { found = moveEv.changedTouches[j]; break; }
                            }
                        }
                        if (!found) return;
                        var meta = touchMeta[found.identifier];
                        if (!meta) return;

                        var dx = found.clientX - meta.startX;
                        var dy = found.clientY - meta.startY;
                        var absDx = Math.abs(dx), absDy = Math.abs(dy);

                        var earlyThreshold = 10;
                        if (!meta.prevented && absDx > earlyThreshold && absDx > absDy) {
                            try {
                                moveEv.preventDefault(); // stops browser back/forward swipe
                                meta.prevented = true;
                                dlog("swipe: prevented default on touchmove (element)", elementKey);
                            } catch (e) {
                                // ignore
                            }
                        } else if (meta.prevented) {
                            try { moveEv.preventDefault(); } catch (e) { }
                        }
                    } catch (e) { /* ignore */ }
                };

                document.addEventListener('touchmove', currentTouchMoveHandler, { passive: false, capture: true });

                dlog("swipe: touchstart registered id", t.identifier, elementKey);
            } catch (e) { dlog("swipe: elementTouchStart error", e); }
        }

        function onElementTouchEnd(ev) {
            try {
                // remove move handler
                if (currentTouchMoveHandler) {
                    document.removeEventListener('touchmove', currentTouchMoveHandler, { capture: true });
                    currentTouchMoveHandler = null;
                }

                if (!ev.changedTouches) return;
                for (var i = 0; i < ev.changedTouches.length; i++) {
                    var ct = ev.changedTouches[i];
                    var meta = touchMeta[ct.identifier];
                    if (!meta) continue;

                    var dx = ct.clientX - meta.startX;
                    var dy = ct.clientY - meta.startY;
                    var dt = Date.now() - meta.startTime;

                    dlog("swipe: touchend id", ct.identifier, "dx", dx, "dy", dy, "dt", dt, "meta.prevented", meta.prevented);

                    delete touchMeta[ct.identifier];

                    if (dt <= allowedTime && Math.abs(dx) >= threshold && Math.abs(dy) <= restraint) {
                        if (dx < 0) {
                            dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /* ignore */ });
                            dlog("swipe: invoke left (touch)", elementKey);
                        } else {
                            dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /* ignore */ });
                            dlog("swipe: invoke right (touch)", elementKey);
                        }
                    }
                }
            } catch (e) { dlog("swipe: elementTouchEnd error", e); }
        }

        function onElementTouchCancel(ev) {
            try {
                if (currentTouchMoveHandler) {
                    document.removeEventListener('touchmove', currentTouchMoveHandler, { capture: true });
                    currentTouchMoveHandler = null;
                }
                if (!ev.changedTouches) return;
                for (var i = 0; i < ev.changedTouches.length; i++) {
                    var ct = ev.changedTouches[i];
                    delete touchMeta[ct.identifier];
                }
                dlog("swipe: touchcancel cleaned", elementKey);
            } catch (e) { /* ignore */ }
        }

        // Attach element-level listeners (we want bubble phase so button clicks still get processed)
        var rec = { el: el, dotNetRef: dotNetRef, handlers: {} };

        if (window.PointerEvent) {
            rec.handlers.pointerdown = onElementPointerDown;
            rec.handlers.pointerup = onElementPointerUp;
            rec.handlers.pointercancel = onElementPointerCancel;

            el.addEventListener('pointerdown', rec.handlers.pointerdown, { passive: true });
            el.addEventListener('pointerup', rec.handlers.pointerup, { passive: true });
            el.addEventListener('pointercancel', rec.handlers.pointercancel, { passive: true });

            rec.mode = 'pointer';
            dlog("swipe: element pointer handlers attached", elementKey);
        } else {
            rec.handlers.touchstart = onElementTouchStart;
            rec.handlers.touchend = onElementTouchEnd;
            rec.handlers.touchcancel = onElementTouchCancel;

            el.addEventListener('touchstart', rec.handlers.touchstart, { passive: false }); // passive:false so we can preventDefault later
            el.addEventListener('touchend', rec.handlers.touchend, { passive: true });
            el.addEventListener('touchcancel', rec.handlers.touchcancel, { passive: true });

            // mouse fallback for desktop
            rec.handlers.mousedown = function (ev) { onElementPointerDown({ pointerId: 'mouse', clientX: ev.clientX, clientY: ev.clientY, pointerType: 'mouse', button: ev.button }); };
            rec.handlers.mouseup = function (ev) { onElementPointerUp({ pointerId: 'mouse', clientX: ev.clientX, clientY: ev.clientY }); };

            el.addEventListener('mousedown', rec.handlers.mousedown, { passive: true });
            el.addEventListener('mouseup', rec.handlers.mouseup, { passive: true });

            rec.mode = 'touchmouse';
            dlog("swipe: element touch handlers attached", elementKey);
        }

        window.openRoseHandlers[elementKey] = rec;

        return true;
    }

    function unregisterElement(element) {
        if (!element) return false;
        var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : null;
        if (!elementKey) return false;
        var rec = window.openRoseHandlers[elementKey];
        if (!rec) return false;
        var el = rec.el;
        var h = rec.handlers || {};
        try {
            if (rec.mode === 'pointer') {
                el.removeEventListener('pointerdown', h.pointerdown);
                el.removeEventListener('pointerup', h.pointerup);
                el.removeEventListener('pointercancel', h.pointercancel);
            } else {
                el.removeEventListener('touchstart', h.touchstart);
                el.removeEventListener('touchend', h.touchend);
                el.removeEventListener('touchcancel', h.touchcancel);
                el.removeEventListener('mousedown', h.mousedown);
                el.removeEventListener('mouseup', h.mouseup);
            }
        } catch (e) { /* ignore */ }

        // cleanup any active touch/pointer metas referencing this element
        for (var pid in window.openRoseHandlers) {
            // nothing per-pointer stored globally here except within element; we used closures, so nothing else to do
        }
        delete window.openRoseHandlers[elementKey];
        try { delete element.dataset.openroseId; } catch (e) { /* ignore */ }
        dlog("swipe: unregistered", elementKey);
        return true;
    }

    // Exposed API for Blazor
    window.openRoseRegisterSwipeElement = function (element, dotNetRef, options) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : ("el-" + Math.random().toString(36).substr(2, 9));
            try { element.dataset.openroseId = elementKey; } catch (e) { /* ignore */ }
            var ok = registerOnElement(element, dotNetRef, elementKey, options || {});
            dlog("swipe: register result for", elementKey, ok);
            return ok;
        } catch (err) { dlog("swipe: register error", err); return false; }
    };

    window.openRoseUnregisterSwipeElement = function (element) {
        try {
            return unregisterElement(element);
        } catch (err) { dlog("swipe: unregister error", err); return false; }
    };

})();