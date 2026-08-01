/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/
// wwwroot/js/swipe.js

// wwwroot/js/swipe.js (production)
(function () {
    if (window.openRoseSwipeLibLoaded) {
        return;
    }
    window.openRoseSwipeLibLoaded = true;

    window.openRoseHandlers = window.openRoseHandlers || {};

    function registerOnElement(el, dotNetRef, elementKey) {
        if (!el) {
            return false;
        }

        var startX = 0, startY = 0, startTime = 0;
        var threshold = 50;
        var restraint = 120;
        var allowedTime = 600;

        function onStart(e) {
            var p = (e.touches && e.touches[0]) || e;
            startX = p.clientX;
            startY = p.clientY;
            startTime = Date.now();
        }

        function onEnd(e) {
            var p = (e.changedTouches && e.changedTouches[0]) || e;
            var distX = p.clientX - startX;
            var distY = p.clientY - startY;
            var elapsed = Date.now() - startTime;

            if (elapsed <= allowedTime && Math.abs(distX) >= threshold && Math.abs(distY) <= restraint) {
                if (distX < 0) {
                    dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /* ignore */ });
                } else {
                    dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /* ignore */ });
                }
            }
        }

        function onPointerDown(e) { onStart(e); }
        function onPointerUp(e) { onEnd(e); }
        function onTouchStart(e) { onStart(e); }
        function onTouchEnd(e) { onEnd(e); }
        function onMouseDown(e) { onStart(e); }
        function onMouseUp(e) { onEnd(e); }

        var uses = [];
        if (window.PointerEvent) {
            el.addEventListener('pointerdown', onPointerDown, { passive: true });
            el.addEventListener('pointerup', onPointerUp, { passive: true });
            uses.push('pointer');
        } else {
            if ('ontouchstart' in window || navigator.maxTouchPoints > 0) {
                el.addEventListener('touchstart', onTouchStart, { passive: true });
                el.addEventListener('touchend', onTouchEnd, { passive: true });
                uses.push('touch');
            }
            el.addEventListener('mousedown', onMouseDown, { passive: true });
            el.addEventListener('mouseup', onMouseUp, { passive: true });
            uses.push('mouse');
        }

        window.openRoseHandlers[elementKey] = {
            el: el,
            handlers: {
                onPointerDown: onPointerDown, onPointerUp: onPointerUp,
                onTouchStart: onTouchStart, onTouchEnd: onTouchEnd,
                onMouseDown: onMouseDown, onMouseUp: onMouseUp
            },
            uses: uses,
            dotNetRef: dotNetRef
        };

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

            var el = rec.el;
            var uses = rec.uses || [];

            try {
                if (uses.indexOf('pointer') >= 0) {
                    el.removeEventListener('pointerdown', rec.handlers.onPointerDown);
                    el.removeEventListener('pointerup', rec.handlers.onPointerUp);
                }
                if (uses.indexOf('touch') >= 0) {
                    el.removeEventListener('touchstart', rec.handlers.onTouchStart);
                    el.removeEventListener('touchend', rec.handlers.onTouchEnd);
                }
                if (uses.indexOf('mouse') >= 0) {
                    el.removeEventListener('mousedown', rec.handlers.onMouseDown);
                    el.removeEventListener('mouseup', rec.handlers.onMouseUp);
                }
            } catch (er) { /* ignore */ }

            delete window.openRoseHandlers[elementKey];
            try { delete element.dataset.openroseId; } catch (e) { }
            return true;
        } catch (err) {
            return false;
        }
    };
})();
