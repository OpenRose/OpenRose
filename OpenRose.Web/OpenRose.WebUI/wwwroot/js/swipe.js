/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

// wwwroot/js/swipe.js
// Production: one-finger swipe detection, preserves pinch/zoom and scrolling.
// Document-level capture listeners, start-inside-element + end-anywhere, pointer+touch fallback.
(function () {
    if (window.openRoseSwipeLibLoaded) {
        return;
    }
    window.openRoseSwipeLibLoaded = true;
    window.openRoseHandlers = window.openRoseHandlers || {};

    function registerOnElement(el, dotNetRef, elementKey) {
        if (!el) return false;

        // Sensitivity parameters (moderate -> more lenient than high sensitivity)
        var threshold = 50;     // px horizontal required to count as swipe
        var restraint = 120;    // px max vertical movement allowed
        var allowedTime = 600;  // ms max duration

        // Pointer tracking
        var activePointers = {};   // pointerId -> { startX, startY, startTime }
        var trackedPointerId = null;

        function onPointerDown(ev) {
            // only start if gesture begins inside element (doc handler ensures this)
            // For touch pointer types ensure we're not already tracking another pointer (multi-touch)
            if (ev.pointerType === 'touch') {
                if (Object.keys(activePointers).length > 0) {
                    // mark presence, but do not track as single-finger
                    activePointers[ev.pointerId] = null;
                    return;
                }
            }

            activePointers[ev.pointerId] = {
                startX: ev.clientX,
                startY: ev.clientY,
                startTime: Date.now()
            };

            if (trackedPointerId === null) trackedPointerId = ev.pointerId;
        }

        function onPointerUp(ev) {
            var meta = activePointers[ev.pointerId];
            // always remove the pointer entry
            delete activePointers[ev.pointerId];

            // Only react if this was the tracked single-finger pointer
            if (trackedPointerId !== ev.pointerId) {
                // if no pointers remain, reset tracked pointer
                if (Object.keys(activePointers).length === 0) trackedPointerId = null;
                return;
            }
            trackedPointerId = null;

            if (!meta) return;

            var distX = ev.clientX - meta.startX;
            var distY = ev.clientY - meta.startY;
            var elapsed = Date.now() - meta.startTime;

            if (elapsed <= allowedTime && Math.abs(distX) >= threshold && Math.abs(distY) <= restraint) {
                if (distX < 0) {
                    // left swipe => Next (book style)
                    dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /* ignore */ });
                } else {
                    // right swipe => Previous
                    dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /* ignore */ });
                }
            }
        }

        function onPointerCancel(ev) {
            // Cleanup
            delete activePointers[ev.pointerId];
            if (trackedPointerId === ev.pointerId) trackedPointerId = null;
        }

        // Touch fallback (for older browsers)
        function onTouchStart(ev) {
            if (!el.contains(ev.target)) return;
            if (ev.touches.length !== 1) {
                el.__swipeTouchMeta = null;
                return;
            }
            var t = ev.touches[0];
            el.__swipeTouchMeta = { startX: t.clientX, startY: t.clientY, startTime: Date.now() };
        }

        function onTouchEnd(ev) {
            var meta = el.__swipeTouchMeta;
            el.__swipeTouchMeta = null;
            if (!meta) return;

            var t = (ev.changedTouches && ev.changedTouches[0]) || null;
            if (!t) return;

            var distX = t.clientX - meta.startX;
            var distY = t.clientY - meta.startY;
            var elapsed = Date.now() - meta.startTime;

            if (elapsed <= allowedTime && Math.abs(distX) >= threshold && Math.abs(distY) <= restraint) {
                if (distX < 0) {
                    dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /* ignore */ });
                } else {
                    dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /* ignore */ });
                }
            }
        }

        function onTouchCancel(ev) {
            el.__swipeTouchMeta = null;
        }

        // Document-level handlers (capture phase). Start only when start inside element.
        function docPointerDown(ev) {
            try {
                if (!el.contains(ev.target)) return;
                onPointerDown(ev);
            } catch (e) { /* ignore */ }
        }
        function docPointerUp(ev) {
            try {
                // Do NOT require ev.target to be inside element; user may lift finger outside.
                onPointerUp(ev);
            } catch (e) { /* ignore */ }
        }
        function docPointerCancel(ev) {
            try {
                onPointerCancel(ev);
            } catch (e) { /* ignore */ }
        }

        function docTouchStart(ev) {
            try {
                if (!el.contains(ev.target)) return;
                onTouchStart(ev);
            } catch (e) { /* ignore */ }
        }
        function docTouchEnd(ev) {
            try {
                onTouchEnd(ev);
            } catch (e) { /* ignore */ }
        }
        function docTouchCancel(ev) {
            try {
                onTouchCancel(ev);
            } catch (e) { /* ignore */ }
        }

        // Attach listeners
        if (window.PointerEvent) {
            document.addEventListener('pointerdown', docPointerDown, { passive: true, capture: true });
            document.addEventListener('pointerup', docPointerUp, { passive: true, capture: true });
            document.addEventListener('pointercancel', docPointerCancel, { passive: true, capture: true });

            window.openRoseHandlers[elementKey] = {
                el: el,
                mode: 'pointer',
                handlers: { docPointerDown: docPointerDown, docPointerUp: docPointerUp, docPointerCancel: docPointerCancel },
                dotNetRef: dotNetRef
            };
        } else {
            // touch + mouse fallback
            document.addEventListener('touchstart', docTouchStart, { passive: true, capture: true });
            document.addEventListener('touchend', docTouchEnd, { passive: true, capture: true });
            document.addEventListener('touchcancel', docTouchCancel, { passive: true, capture: true });

            // mouse fallback (desktop)
            document.addEventListener('mousedown', docPointerDown, { passive: true, capture: true });
            document.addEventListener('mouseup', docPointerUp, { passive: true, capture: true });

            window.openRoseHandlers[elementKey] = {
                el: el,
                mode: 'touchmouse',
                handlers: {
                    docTouchStart: docTouchStart,
                    docTouchEnd: docTouchEnd,
                    docTouchCancel: docTouchCancel,
                    docPointerDown: docPointerDown,
                    docPointerUp: docPointerUp
                },
                dotNetRef: dotNetRef
            };
        }

        return true;
    }

    window.openRoseRegisterSwipeElement = function (element, dotNetRef) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : ("el-" + Math.random().toString(36).substr(2, 9));
            try { element.dataset.openroseId = elementKey; } catch (e) { }
            return registerOnElement(element, dotNetRef, elementKey);
        } catch (err) {
            return false;
        }
    };

    window.openRoseUnregisterSwipeElement = function (element) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : null;
            if (!elementKey) return false;
            var rec = window.openRoseHandlers[elementKey];
            if (!rec) return false;

            var mode = rec.mode;
            var h = rec.handlers || {};
            try {
                if (mode === 'pointer') {
                    document.removeEventListener('pointerdown', h.docPointerDown, { capture: true });
                    document.removeEventListener('pointerup', h.docPointerUp, { capture: true });
                    document.removeEventListener('pointercancel', h.docPointerCancel, { capture: true });
                } else {
                    document.removeEventListener('touchstart', h.docTouchStart, { capture: true });
                    document.removeEventListener('touchend', h.docTouchEnd, { capture: true });
                    document.removeEventListener('touchcancel', h.docTouchCancel, { capture: true });
                    document.removeEventListener('mousedown', h.docPointerDown, { capture: true });
                    document.removeEventListener('mouseup', h.docPointerUp, { capture: true });
                }
            } catch (er) { /* ignore */ }

            delete window.openRoseHandlers[elementKey];
            try { delete element.dataset.openroseId; } catch (e) { }
            try { delete element.__swipeTouchMeta; } catch (e) { }
            return true;
        } catch (err) {
            return false;
        }
    };
})();