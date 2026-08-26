$(function () {
    let Keyboard = window.SimpleKeyboard.default;

    let keyboard = new Keyboard({
        onChange: input => onChange(input),
        onKeyPress: button => onKeyPress(button),
        theme: "hg-theme-default hg-theme-ios",
        layout: {
            default: [
                "q w e r t y u i o p {backspace}",
                "a s d f g h j k l {enter}",
                "{shiftleft} z x c v b n m , . {shiftright}",
                "{altleft} {space} {altright} {downkeyboard}"
            ],
            shift: [
                "Q W E R T Y U I O P {backspace}",
                "A S D F G H J K L {enter}",
                "{shiftleftactivated} Z X C V B N M ! ? {shiftrightactivated}",
                "{altleft} {space} {altright} {downkeyboard}"
            ],
            alt: [
                "1 2 3 4 5 6 7 8 9 0 {backspace}",
                `@ # $ & * ( ) ' " {enter}`,
                "{shiftleft} % _ - + = / ~ ; : {shiftright}",
                "{defaultleft} {space} {defaultright} {downkeyboard}"
            ],
            numbers: [
                "7 8 9 {backspace}",
                "4 5 6 {enter}",
                "1 2 3 -",
                "0 . {downkeyboard}"
            ]
        },
        display: {
            "{backspace}": "<icon class='icon icon-keyboard icon-backspace'></icon>",
            "{enter}": "<icon class='icon icon-keyboard icon-enter'></icon>",
            "{shiftleft}": "<icon class='icon icon-keyboard icon-shift'></icon>",
            "{shiftright}": "<icon class='icon icon-keyboard icon-shift'></icon>",
            "{shiftleftactivated}": "<icon class='icon icon-keyboard icon-shiftactivated'></icon>",
            "{shiftrightactivated}": "<icon class='icon icon-keyboard icon-shiftactivated'></icon>",
            "{altleft}": ".?123",
            "{altright}": ".?123",
            "{settings}": "<icon class='icon icon-keyboard icon-settings'></icon>",
            "{downkeyboard}": "<icon class='icon icon-keyboard icon-keyboarddown'></icon>",
            "{space}": " ",
            "{defaultleft}": "ABC",
            "{defaultright}": "ABC"
        },
        useMouseEvents: true,
        preventMouseDownDefault: true,
        physicalKeyboardHighlight: true,
        physicalKeyboardHighlightPress: false
    });

    let lazyHideKeyboard;

    function onChange(input) {
        let caretPosition = keyboard.getCaretPosition();
        $(".input-target").val(input).trigger("input");
        // Set caret position
        $(".input-target")[0].setSelectionRange(caretPosition, caretPosition);
    }

    function onKeyPress(button) {
        // Hide keyboard when click enter or downkeyboard
        if (button === "{downkeyboard}" || button === "{enter}") {
            setTimeout(() => {
                $(".simple-keyboard").slideUp(100, () => {
                    if ($(".input-target").closest(".modal").length) {
                        $(".modal").removeClass("modal-top");
                    }
                });
                $(".input-target").blur();
            }, 200);
        }
        // Toggle layout
        if (button.includes("{") && button.includes("}")) {
            handleLayoutChange(button);
        }
    }

    function handleLayoutChange(button) {
        let layoutName;

        switch (button) {
            case "{shiftleft}":
            case "{shiftright}":
                layoutName = "shift";
                break;

            case "{defaultleft}":
            case "{defaultright}":
            case "{shiftleftactivated}":
            case "{shiftrightactivated}":
                layoutName = "default";
                break;

            case "{altleft}":
            case "{altright}":
                layoutName = "alt";
                break;

            default:
                break;
        }

        if (layoutName) {
            keyboard.setOptions({
                layoutName: layoutName
            });
        }
    }

    function scrollToTarget(objTarget, objScrollContainer) {
        const objTargetTop = objTarget.offset().top;
        const objScrollContainerTop = objScrollContainer.offset().top;
        const objTargetHeight = parseInt(objTarget.css("height"));
        const objScrollContainerHeight = parseInt(objScrollContainer.css("height"));
        const scrollTop = objScrollContainer.scrollTop();
        let newScrollTop = (objTargetTop + objTargetHeight / 2) - (objScrollContainerTop + objScrollContainerHeight / 2) + scrollTop;
        newScrollTop = newScrollTop > 0 ? newScrollTop : 0;
        objScrollContainer.animate({ scrollTop: newScrollTop }, 200);
    }

    // Update simple-keyboard when input is changed directly
    $(document).on("input", ".form-control.form-line", function (event) {
        keyboard.setInput(event.target.value);
    });

    // Show keyboard
    $(document).on("focus", ".form-control.form-line", function () {
        // Date control doesn't show keyboard
        if ($(this).parent(".form-date-single").length) {
            return false;
        }

        let layoutName;
        clearTimeout(lazyHideKeyboard);
        $(".input-target").not(this).removeClass("input-target");
        $(this).addClass("input-target");

        $(".simple-keyboard").slideDown(200, () => {
            if ($(".input-target").length) {
                const obj = $(".input-target").eq(0);
                if (obj.closest(".modal").length) {
                    $(".modal").addClass("modal-top");
                    scrollToTarget(obj, obj.closest(".modal-body"));
                } else {
                    scrollToTarget(obj, obj.closest(".main-box-body"));
                }
            }
        });

        keyboard.setInput($(this).val());

        if ($(this).hasClass("form-number")) {
            layoutName = "numbers";
        } else if ($(this).hasClass("form-number-first")) {
            layoutName = "alt";
        } else {
            layoutName = "default";
        }

        if (layoutName) {
            keyboard.setOptions({
                layoutName: layoutName
            });
        }
    });

    // Hide keyboard
    $(document).on("blur", ".form-control.form-line", function () {
        $(this).trigger("change");
        lazyHideKeyboard = setTimeout(() => {
            $(".simple-keyboard").slideUp(100, () => {
                if ($(".input-target").closest(".modal").length) {
                    $(".modal").removeClass("modal-top");
                }
            });
        }, 100);
    });
});
