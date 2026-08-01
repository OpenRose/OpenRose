/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

// Production-ready: one-finger swipe detection (medium sensitivity), preserves pinch/zoom and scrolling.
(function () {
    if (window.openRoseSwipeLibLoaded) {
        return;
    }
    window.openRoseSwipeLibLoaded = true;
    window.openRoseHandlers = window.openRoseHandlers || {};

    function registerOnElement(el, dotNetRef, elementKey) {
        if (!el) return false;

        // Medium sensitivity parameters
        var threshold = 80;     // min horizontal movement (px) to count as swipe
        var restraint = 150;    // max vertical movement allowed (px)
        var allowedTime = 700;  // max ms for the swipe gesture

        // For pointer events tracking
        var activePointers = {}; // map pointerId => {startX,startY,startTime}
        var trackedPointerId = null; // pointer we are tracking for this element

        function onPointerDown(ev) {
            // Only track single-finger touch (pointerType === 'touch') or mouse left button.
            // If multiple pointers exist, we ignore to preserve multi-finger gestures (pinch).
            if (ev.pointerType === 'touch') {
                // If there are any active pointers already, don't start a new one (multi-touch)
                if (Object.keys(activePointers).length > 0) {
                    activePointers[ev.pointerId] = null; // mark but don't track
                    return;
                }
            }

            // record start values
            activePointers[ev.pointerId] = {
                startX: ev.clientX,
                startY: ev.clientY,
                startTime: Date.now()
            };

            // If this is the first/only pointer, mark it tracked
            if (trackedPointerId === null) trackedPointerId = ev.pointerId;
        }

        function onPointerUp(ev) {
            var meta = activePointers[ev.pointerId];
            // Clean up the activePointers entry (we'll compute before deleting)
            delete activePointers[ev.pointerId];

            // Only react if this pointer was the tracked one and we had no other pointers when it started
            if (trackedPointerId !== ev.pointerId) {
                // if the pointer we tracked isn't this one, ignore
                if (Object.keys(activePointers).length === 0) trackedPointerId = null;
                return;
            }

            trackedPointerId = null; // reset tracked pointer

            if (!meta) return; // not a tracked single-finger start

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

        // Touch fallback for browsers not using PointerEvent
        // We will ignore multi-touch (touches.length !== 1) to preserve pinch gestures
        function onTouchStart(ev) {
            if (!el.contains(ev.target)) return;
            if (ev.touches.length !== 1) {
                // multi-touch start -> ignore for swipe detection
                el.__swipeTouchMeta = null;
                return;
            }
            var t = ev.touches[0];
            el.__swipeTouchMeta = { startX: t.clientX, startY: t.clientY, startTime: Date.now() };
        }

        function onTouchEnd(ev) {
            if (!el.__swipeTouchMeta) return;
            var t = (ev.changedTouches && ev.changedTouches[0]) || null;
            if (!t) { el.__swipeTouchMeta = null; return; }

            var meta = el.__swipeTouchMeta;
            el.__swipeTouchMeta = null;

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

        // Document-level handlers in capture phase, but we still ensure gesture starts inside element.
        // We use capture so parent/child elements that call stopPropagation in bubble phase won't block us.
        function docPointerDown(ev) {
            try {
                if (!el.contains(ev.target)) return;
                onPointerDown(ev);
            } catch (e) { /* ignore */ }
        }
        function docPointerUp(ev) {
            try {
                if (!el.contains(ev.target)) return;
                onPointerUp(ev);
            } catch (e) { /* ignore */ }
        }
        function docTouchStart(ev) {
            try {
                // Only care if touchstart happened inside element
                if (!el.contains(ev.target)) return;
                onTouchStart(ev);
            } catch (e) { /* ignore */ }
        }
        function docTouchEnd(ev) {
            try {
                // touchend target may be different; use the stored meta only
                onTouchEnd(ev);
            } catch (e) { /* ignore */ }
        }

        // Attach handlers
        if (window.PointerEvent) {
            document.addEventListener('pointerdown', docPointerDown, { passive: true, capture: true });
            document.addEventListener('pointerup', docPointerUp, { passive: true, capture: true });
            // Store record
            window.openRoseHandlers[elementKey] = {
                el: el,
                mode: 'pointer',
                handlers: { docPointerDown: docPointerDown, docPointerUp: docPointerUp },
                dotNetRef: dotNetRef
            };
        } else {
            // touch + mouse fallback
            document.addEventListener('touchstart', docTouchStart, { passive: true, capture: true });
            document.addEventListener('touchend', docTouchEnd, { passive: true, capture: true });
            document.addEventListener('mousedown', docPointerDown, { passive: true, capture: true });
            document.addEventListener('mouseup', docPointerUp, { passive: true, capture: true });

            window.openRoseHandlers[elementKey] = {
                el: el,
                mode: 'touchmouse',
                handlers: {
                    docTouchStart: docTouchStart,
                    docTouchEnd: docTouchEnd,
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
                } else {
                    document.removeEventListener('touchstart', h.docTouchStart, { capture: true });
                    document.removeEventListener('touchend', h.docTouchEnd, { capture: true });
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