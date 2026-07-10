/**
 * roc-datepicker.js
 *
 * bootstrap-datepicker 民國年共用元件
 * ─────────────────────────────────────────────────
 * 使用方式：
 *   日期模式：$('#birthDate').datepicker();
 *   年月模式：$('#startMonth').datepicker({ minViewMode: 'months' });
 *   自訂覆蓋：$('#custom').datepicker({ orientation: 'top auto' });
 *
 * 工具函式（格式統一使用 '-' 分隔、月日補零）：
 *   window.RocDatepicker.toRocDateStr(date)        → '113-01-30'  （Date → 民國字串）
 *   window.RocDatepicker.toRocMonthStr(date)       → '113-01'     （Date → 民國年月字串）
 *   window.RocDatepicker.rocStrToDate('113-01-30') → Date         （民國字串 → Date）
 *   window.RocDatepicker.rocStrToMonth('113-01')   → Date         （民國年月字串 → Date）
 *   window.RocDatepicker.toAdDateStr(date)         → '2024-01-30' （Date → 西元字串）
 *   window.RocDatepicker.toAdMonthStr(date)        → '2024-01'    （Date → 西元年月字串）
 * ─────────────────────────────────────────────────
 */
(function ($) {
    'use strict';

    // ==================== 工具函式 ====================
    // 格式統一：'-' 分隔、月日補零（例：113-01-30、113-01）

    /** Date → 民國日期字串 '113-01-30' */
    function toRocDateStr(date) {
        if (!date) return '';
        var y = date.getFullYear() > 1911 ? date.getFullYear() - 1911 : date.getFullYear();
        var m = (date.getMonth() + 1).toString().padStart(2, '0');
        var d = date.getDate().toString().padStart(2, '0');
        return y + '-' + m + '-' + d;
    }

    /** Date → 民國年月字串 '113-01' */
    function toRocMonthStr(date) {
        if (!date) return '';
        var y = date.getFullYear() > 1911 ? date.getFullYear() - 1911 : date.getFullYear();
        var m = (date.getMonth() + 1).toString().padStart(2, '0');
        return y + '-' + m;
    }

    /** 民國日期字串 '113-01-30' → Date（也相容 '113/01/30'） */
    function rocStrToDate(rocStr) {
        if (!rocStr) return null;
        var parts = rocStr.split(/[-/]/);
        if (parts.length !== 3) return null;
        var y = parseInt(parts[0], 10) + 1911;
        var m = parseInt(parts[1], 10) - 1;
        var d = parseInt(parts[2], 10);
        return new Date(y, m, d);
    }

    /** 民國年月字串 '113-01' → Date（也相容 '113/01'） */
    function rocStrToMonth(rocStr) {
        if (!rocStr) return null;
        var parts = rocStr.split(/[-/]/);
        if (parts.length !== 2) return null;
        var y = parseInt(parts[0], 10) + 1911;
        var m = parseInt(parts[1], 10) - 1;
        return new Date(y, m, 1);
    }

    /** Date → 西元日期字串 '2024-01-30' */
    function toAdDateStr(dateObj) {
        if (!dateObj) return '';
        var y = dateObj.getFullYear();
        var m = (dateObj.getMonth() + 1).toString().padStart(2, '0');
        var d = dateObj.getDate().toString().padStart(2, '0');
        return y + '-' + m + '-' + d;
    }

    /** Date → 西元年月字串 '2024-01' */
    function toAdMonthStr(date) {
        if (!(date instanceof Date)) return '';
        var y = date.getFullYear();
        var m = (date.getMonth() + 1).toString().padStart(2, '0');
        return y + '-' + m;
    }

    // ==================== 輸入遮罩 ====================

    /**
     * 民國日期輸入遮罩（即時顯示剩餘欄位）
     *
     * 日期模式：___-__-__  →  1__-__-__  →  114-05-05
     * 年月模式：___-__     →  1__-__     →  114-05
     *
     * @param {jQuery} $el   - input 元素
     * @param {string} mode  - 'date' 或 'month'
     */
    function applyRocMask($el, mode) {
        var template   = (mode === 'month') ? '___-__' : '___-__-__';
        var digitSlots = (mode === 'month') ? 5 : 7;

        $el.attr('placeholder', template);
        $el.attr('maxlength', template.length);
        $el.attr('inputmode', 'numeric');

        // 從 template 取得數字位置索引 [0,1,2,4,5] 或 [0,1,2,4,5,7,8]
        var digitPositions = [];
        for (var i = 0; i < template.length; i++) {
            if (template[i] === '_') digitPositions.push(i);
        }

        /** 純數字 → 帶遮罩的顯示字串（剩餘位補 '_'） */
        function buildDisplay(digits) {
            var padded = digits;
            while (padded.length < digitSlots) padded += '_';
            var result = '';
            var di = 0;
            for (var i = 0; i < template.length; i++) {
                result += (template[i] === '_') ? padded[di++] : template[i];
            }
            return result;
        }

        /** 取出值中的純數字 */
        function getDigits(val) {
            return val.replace(/[^\d]/g, '');
        }

        /** 找到第一個 '_' 的位置（游標應停在此處） */
        function nextEmptyPos(display) {
            var pos = display.indexOf('_');
            return pos === -1 ? display.length : pos;
        }

        // ── 鍵盤事件：攔截按鍵，逐字處理 ──
        $el.on('keydown', function (e) {
            var el = this;
            var digits = getDigits(el.value);

            // Backspace：刪除最後一位數字
            if (e.key === 'Backspace') {
                e.preventDefault();
                if (digits.length > 0) {
                    digits = digits.slice(0, -1);
                    el.value = digits.length ? buildDisplay(digits) : '';
                    if (digits.length) el.setSelectionRange(nextEmptyPos(el.value), nextEmptyPos(el.value));
                }
                return;
            }

            // 允許：Tab, Enter, Escape, 方向鍵, Home, End
            if (['Tab', 'Enter', 'Escape', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'].indexOf(e.key) !== -1) return;
            // 允許：Ctrl/Cmd + A, C, V, X
            if ((e.ctrlKey || e.metaKey) && ['a', 'c', 'v', 'x'].indexOf(e.key.toLowerCase()) !== -1) return;

            // 非數字全部擋掉
            if (!/^\d$/.test(e.key)) {
                e.preventDefault();
                return;
            }

            // 數字鍵：填入下一個空位
            e.preventDefault();
            if (digits.length < digitSlots) {
                digits += e.key;
                el.value = buildDisplay(digits);
                el.setSelectionRange(nextEmptyPos(el.value), nextEmptyPos(el.value));
            }
        });

        // ── 貼上事件 ──
        $el.on('paste', function (e) {
            e.preventDefault();
            var pasted = (e.originalEvent.clipboardData || window.clipboardData).getData('text');
            var digits = pasted.replace(/[^\d]/g, '').substring(0, digitSlots);
            if (!digits.length) { this.value = ''; return; }
            this.value = buildDisplay(digits);
            this.setSelectionRange(nextEmptyPos(this.value), nextEmptyPos(this.value));
        });

        // ── input 事件（IME、語音輸入等備援） ──
        $el.on('input', function () {
            var digits = getDigits(this.value).substring(0, digitSlots);
            if (!digits.length) { this.value = ''; return; }
            this.value = buildDisplay(digits);
            this.setSelectionRange(nextEmptyPos(this.value), nextEmptyPos(this.value));
        });

        // ── focus：游標定位到下一個空位 ──
        $el.on('focus', function () {
            var el = this;
            setTimeout(function () {
                if (el.value && el.value.indexOf('_') !== -1) {
                    el.setSelectionRange(nextEmptyPos(el.value), nextEmptyPos(el.value));
                }
            }, 0);
        });
    }

    // ==================== 全域預設值 ====================

    // 覆寫 zh-TW 語系的日期格式（原始為 'yyyy/mm/dd'，統一改用 '-' 分隔）
    // bootstrap-datepicker 優先順序：實例選項 > 語系格式 > 全域預設
    // 若不覆寫，語系的 '/' 會蓋掉全域預設的 '-'
    if ($.fn.datepicker.dates && $.fn.datepicker.dates['zh-TW']) {
        $.fn.datepicker.dates['zh-TW'].format = 'yyyy-mm-dd';
    }

    $.extend($.fn.datepicker.defaults, {
        format:          'yyyy-mm-dd',
        language:        'zh-TW',
        autoclose:       true,
        todayHighlight:  true,
        orientation:     'bottom auto'
    });

    // ==================== 自訂 datepicker 格式====================

    var _original = $.fn.datepicker;

    $.fn.datepicker = function (option) {
        // 呼叫方法（如 'update', 'getDate', 'destroy'）直接轉發
        if (typeof option === 'string') {
            return _original.apply(this, arguments);
        }

        var opts = $.extend({}, option);

        // 年月模式：自動補齊 format 和 startView
        if (opts.minViewMode === 'months') {
            if (!opts.format)    opts.format = 'yyyy-mm';
            if (!opts.startView) opts.startView = 'months';
        }

        // 是否停用輸入遮罩（預設啟用）
        var disableMask = (opts.mask === false);
        delete opts.mask;

        // 初始化原始 datepicker
        var result = _original.call(this, opts);

        // 自動綁定民國年轉換 + 輸入遮罩（只綁一次）
        var isMonthMode = (opts.minViewMode === 'months');

        this.each(function () {
            var $el = $(this);
            if ($el.data('roc-bound')) return;
            $el.data('roc-bound', true);

            // 輸入遮罩
            if (!disableMask) {
                applyRocMask($el, isMonthMode ? 'month' : 'date');
            }

            // 民國年自動轉換
            $el.on('changeDate', function (e) {
                if (!e.date) return;
                var year = e.date.getFullYear();
                if (year < 1912) return;

                var rocYear = year - 1911;
                var month = String(e.date.getMonth() + 1).padStart(2, '0');
                var rocStr;

                if (isMonthMode) {
                    rocStr = rocYear + '-' + month;
                } else {
                    var day = String(e.date.getDate()).padStart(2, '0');
                    rocStr = rocYear + '-' + month + '-' + day;
                }

                if ($el.val() !== rocStr) {
                    _original.call($el, 'update', rocStr);
                }
            });
        });

        return result;
    };

    // 保留原始 datepicker 的靜態屬性（defaults, Constructor, DPGlobal 等）
    for (var prop in _original) {
        if (_original.hasOwnProperty(prop)) {
            $.fn.datepicker[prop] = _original[prop];
        }
    }

    // ==================== 共用工具函式 ====================

    window.RocDatepicker = {
        toRocDateStr:   toRocDateStr,    // Date → '113-01-30'
        toRocMonthStr:  toRocMonthStr,   // Date → '113-01'
        rocStrToDate:   rocStrToDate,    // '113-01-30' → Date
        rocStrToMonth:  rocStrToMonth,   // '113-01'    → Date
        toAdDateStr:    toAdDateStr,     // Date → '2024-01-30'
        toAdMonthStr:   toAdMonthStr     // Date → '2024-01'
    };

})(jQuery);
