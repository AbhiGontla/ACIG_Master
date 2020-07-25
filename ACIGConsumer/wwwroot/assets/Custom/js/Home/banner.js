"use strict";
// Class definition

var KTBanners = function () {
    // Private functions
    var formEl;
    var submitBtn;
    // basic demo
    var init = function () {
        var avatar5 = new KTImageInput('kt_image_5');
        avatar5.on('cancel', function (imageInput) {
            swal.fire({
                title: 'Image successfully changed !',
                type: 'success',
                buttonsStyling: false,
                confirmButtonText: 'Awesome!',
                confirmButtonClass: 'btn btn-primary font-weight-bold'
            });
        });

        avatar5.on('change', function (imageInput) {
            swal.fire({
                title: 'Image successfully changed !',
                type: 'success',
                buttonsStyling: false,
                confirmButtonText: 'Awesome!',
                confirmButtonClass: 'btn btn-primary font-weight-bold'
            });
        });

        avatar5.on('remove', function (imageInput) {
            swal.fire({
                title: 'Image successfully removed !',
                type: 'error',
                buttonsStyling: false,
                confirmButtonText: 'Got it!',
                confirmButtonClass: 'btn btn-primary font-weight-bold'
            });
        });
        submitBtn.click(function (e){
            var form = $('#kt_form').serialize();
            $('#kt_form').ajaxSubmit({
                success: function (response, status, xhr, $form) {
                    if (response.success) {
                        swal.fire({
                            title: 'Banner successfully saved !',
                            type: 'success',
                            buttonsStyling: false,
                            confirmButtonText: 'Awesome!',
                            confirmButtonClass: 'btn btn-primary font-weight-bold'
                        });
                    } else {
                        swal.fire({
                            title: 'Something went wrong !',
                            type: 'error',
                            buttonsStyling: false,
                            confirmButtonText: 'Got it!',
                            confirmButtonClass: 'btn btn-primary font-weight-bold'
                        });
                    }

                }
            });
        });
    };

    return {
        // public functions
        init: function () {
            formEl = $('#kt_form');
            submitBtn = $('#btnbanner_submit');
            init();
        },
    };
}();
"use strict";
// Class definition
jQuery(document).ready(function () {
    KTBanners.init();
});

