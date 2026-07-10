(function ($) {
    'use strict';

    var $picker = null;

    function initPicker() {
        if ($picker) return;
        $picker = $('<div class="ehris-tp-dropdown"><div class="ehris-tp-col hours"></div><div class="ehris-tp-col mins"></div></div>');
        for (var h = 0; h < 24; h++) $picker.find('.hours').append('<div>' + h.toString().padStart(2, '0') + '</div>');
        for (var m = 0; m < 60; m++) $picker.find('.mins').append('<div>' + m.toString().padStart(2, '0') + '</div>');
        $('body').append($picker);
    }

    function applyTimeMask($el) {
        initPicker();

        var template = "__:__";
        $el.attr('placeholder', template);
        $el.attr('maxlength', 5);
        $el.attr('inputmode', 'numeric');

        var currentVal = ($el.val() || "").trim();
        if (currentVal === "" || currentVal === "null" || currentVal === "__:__") {
            $el.val("");
        }

        $el.on('click', function (e) {
            var offset = $(this).offset();
            var panelWidth = $picker.outerWidth() || 150;
            $picker.css({
                top: offset.top + $(this).outerHeight() + 2,
                left: offset.left
            }).show();
            $picker.data('target', $(this));
            e.stopPropagation();
        });

        function buildDisplay(digits) {
            var d = digits.split('');
            var r = ['_', '_', ':', '_', '_'];
            if (d[0]) r[0] = d[0];
            if (d[1]) r[1] = d[1];
            if (d[2]) r[3] = d[2];
            if (d[3]) r[4] = d[3];
            return r.join('');
        }

        function getDigits(val) {
            return val.replace(/[^\d]/g, '').substring(0, 4);
        }

        $el.on('keydown', function (e) {
            var el = this;
            var digits = getDigits(el.value);

            if (e.key === 'Backspace') {
                e.preventDefault();
                if (digits.length > 0) {
                    digits = digits.slice(0, -1);
                    el.value = digits.length ? buildDisplay(digits) : '';
                    var pos = el.value.indexOf('_');
                    var finalPos = pos === -1 ? el.value.length : pos;
                    el.setSelectionRange(finalPos, finalPos);
                }
                return;
            }

            if (['Tab', 'Enter', 'Escape', 'ArrowLeft', 'ArrowRight'].indexOf(e.key) !== -1) return;

            if (!/^\d$/.test(e.key)) {
                e.preventDefault();
                return;
            }

            e.preventDefault();
            if (digits.length < 4) {
                var nextDigit = e.key;
                var testDigits = digits + nextDigit;

                if (testDigits.length === 1 && parseInt(testDigits) > 2) return;
                if (testDigits.length === 2 && parseInt(testDigits) > 23) return;
                if (testDigits.length === 3 && parseInt(nextDigit) > 5) return;

                digits = testDigits;
                el.value = buildDisplay(digits);

                var p = el.value.indexOf('_');
                var setP = p === -1 ? el.value.length : p;
                el.setSelectionRange(setP, setP);
                $(el).trigger('change');
            }
        });

        $el.on('blur', function () {
            var digits = getDigits(this.value);
            if (digits.length > 0 && digits.length < 4) {
                var full = digits.padEnd(4, '0');
                this.value = buildDisplay(full);
                $(this).trigger('change');
            }
        });
    }

    $(document).on('click', '.ehris-tp-col div', function (e) {
        e.stopPropagation();
        var $col = $(this).parent();
        var val = $(this).text();
        var $target = $picker.data('target');
        var current = $target.val().replace(/_/g, '0');
        if (current === '' || current === '__:__') current = '00:00';

        var parts = current.split(':');
        if ($col.hasClass('hours')) {
            parts[0] = val;
            $col.find('div').removeClass('selected');
            $(this).addClass('selected');
        } else {
            parts[1] = val;
            $col.find('div').removeClass('selected');
            $(this).addClass('selected');
            $picker.hide();
        }

        $target.val(parts.join(':')).trigger('change');
    });

    $(document).on('click', function () {
        if ($picker) $picker.hide();
    });

    $.fn.timepicker = function () {
        return this.each(function () {
            var $el = $(this);
            if ($el.data('time-bound')) return;
            $el.data('time-bound', true);
            $el.attr('type', 'text').attr('autocomplete', 'off');
            applyTimeMask($el);
        });
    };
})(jQuery);