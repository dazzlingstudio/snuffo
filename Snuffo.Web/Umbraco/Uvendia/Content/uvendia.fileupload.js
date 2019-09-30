Umbraco.Sys.registerNamespace("Uvendia.Media");

$(function () {
    'use strict';
    var fileType = $('.media-container').data('cloudinary-type');

    // Change this to the location of your server-side upload handler:    
    var uploadButton = $('<button/>')
        .addClass('btn btn-primary')
        .prop('disabled', true)
        .text('Processing...')
        .attr('type', 'button')
        .on('click', function () {
            var uploadUrl = '/Umbraco/Backoffice/Uvendia/Base/UploadToCloudinary';
            if ($('#Folders').length) {
                var folders = $('#Folders').val();
                uploadUrl = uploadUrl + '?folders=' + folders + '&fileType=' + fileType;
            }
            else {
                uploadUrl = uploadUrl + '?fileType=' + fileType;
            }
            
            $('.fileupload').fileupload('option', 'url', uploadUrl);                
            var $this = $(this),
                data = $this.data();
            $this
                .off('click')
                .text('Abort')
                .on('click', function () {
                    $this.remove();
                    data.abort();
                });
            data.submit().always(function () {                
                $this.remove();
            });
        });
    var resetButton = $('<button/>')
            .attr('id', 'btnResetUpload')
            .addClass('btn btn-warning')
            .html('<i class="glyphicon glyphicon-ban-circle"></i> Reset')
            .attr('type', 'reset')
            .on('click', function () {
                var $this = $(this),
                    data = $this.data();
                data.abort();
                data.context.remove();
                $('.addImage-body #select-fileupload-container').show();
                $('.addImage-body #image_upload_preview').hide();
                $('.addImage-body #progress').hide();
                $(".fileupload").val('');
            });

    $('.fileupload').fileupload({
        paramName: 'files[]',
        datatype: 'json',
        autoUpload: false,
        //acceptFileTypes: /(\.|\/)(gif|jpe?g|png|tif|TIF)$/i,
        maxFileSize: 1070081999,
        // Enable image resizing, except for Android and Opera,
        // which actually support image resizing, but fail to
        // send Blob objects via XHR requests:
        //disableImageResize: /Android(?!.*Chrome)|Opera/
        //    .test(window.navigator.userAgent),   
        disableImageResize: true,
        previewCanvas: false
    }).on('fileuploadadd', function (e, data) {
        $('#select-fileupload-container').hide();
        
        data.context = $('<div/>').appendTo('#files');
        $.each(data.files, function (index, file) {
            var node = $('<p/>')
                    .append($('<span/>').text(file.name));
            if (!index) {
                node
                    .append('<br>')
                    .append(uploadButton.clone(true).data(data))
                    .append('&nbsp;')
                    .append(resetButton.clone(true).data(data));                     
            }
            node.appendTo(data.context);
        });
    }).on('fileuploadprocessalways', function (e, data) {
        var index = data.index,
            file = data.files[index],
            node = $(data.context.children()[index]);        

        // Show preview image only for upload-type 'image'
        
        var img = $('.media-container #image_upload_preview');

        if (fileType === 'image') {
            var reader = new FileReader();            
            img.show();
            reader.onload = function (e) {
                img.attr('src', e.target.result)
                    .attr('width', 844);
            };
            reader.readAsDataURL(file);
        }
        else {
            img.attr('src', '/Umbraco/Uvendia/Content/images/upload.png');
            img.show();
        }       

        if (file.error) {
            node
                .append('<br>')
                .append($('<span class="text-danger"/>').text(file.error));
        }
        if (index + 1 === data.files.length) {
            data.context.find('button.btn-primary')
                .html('<i class="glyphicon glyphicon-upload"></i> Upload')
                .prop('disabled', !!data.files.error);
        }
        }).on('fileuploadprogressall', function (e, data) {            

            $('#wait').show();
            $('.media-container #btnResetUpload').hide();

            $('.media-container #progress').show();
            var progress = parseInt(data.loaded / data.total * 100, 10);
            $('.media-container #progress .progress-bar').css(
                'width',
                progress + '%'
            );
        }).on('fileuploaddone', function (e, data) {
            $('#wait').hide();
            $('.media-container #btnResetUpload').hide();
            var succeed = $('<i/>').addClass('btn btn-success btn-lg glyphicon glyphicon-ok');
            $(data.context[0]).append(succeed);

            if (data.result) {
                // Save the returned json in a hidden field.
                var hiddenInputSucceedJson = $('<input/>', {
                    type: "hidden",
                    id: "hdnCloudinaryJson",
                    value: JSON.stringify(data.result)
                });
                $(data.context[0]).append(hiddenInputSucceedJson);
                $('button.saveCloseCloudinaryModal').removeAttr('disabled');

                //var json = JSON.parse(data.result);
                // Refresh container
                //Eventshop.UI.Media.LoadMedia(json.EventID, json.MediaTypeID);
            }

            $.each(data.result.files, function (index, file) {
                if (file.url) {
                    var link = $('<a>')
                        .attr('target', '_blank')
                        .prop('href', file.url);
                    $(data.context.children()[index])
                        .wrap(link);
                } else if (file.error) {
                    var error = $('<span class="text-danger"/>').text(file.error);
                    $(data.context.children()[index])
                        .append('<br>')
                        .append(error);
                }
            });
    }).on('fileuploadfail', function (e, data) {
        $.each(data.files, function (index) {
            console.log('data', data);
            $('#wait').hide();
            var error = $('<span class="text-danger"/>').text('Unhandled error occurred!');
            $(data.context.children()[index])
                .append('<br>')
                .append('<br>')
                .append(error);
        });
    }).prop('disabled', !$.support.fileInput)
        .parent().addClass($.support.fileInput ? undefined : 'disabled');
});

; (function ($) {
    Uvendia.Media = {
        loadPictures: function () {
        }
    }
})(jQuery);