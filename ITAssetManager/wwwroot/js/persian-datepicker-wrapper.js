function initPersianDatePicker(selector) {

    $(selector).each(function () {

        $(this).persianDatepicker({
            format: 'YYYY/MM/DD',
            autoClose: true,
            initialValue: false,
            calendar: {
                persian: {
                    locale: 'fa'
                }
            },
            timePicker: {
                enabled: false
            }
        });

    });

}