
Umbraco.Sys.registerNamespace("Uvendia");

if (typeof ready === "undefined") {
    var ready = {
        jquery: false
    };
}

$(function () {

    if (ready.jquery) {        
        return;
    }

    ready.jquery = true;

    $(".nav-tabs a").click(function (e) {
        e.preventDefault();
    });

    if ($("#tabs").length) {
        $("#tabs").tabs();
        // Keep selected tab on post
        var currentIndex = $('#CurrentTabIndex');
        if (currentIndex.length) {
            Uvendia.selectTab(currentIndex.val());

            $('.nav-tabs a').on('shown.bs.tab', function (event) {
                var idx = $(this).index('a[data-toggle="tab"]');
                currentIndex.val(idx);
            });
        }
    }

    var alertMessage = $("#hdnWebAlertMessage").val();
    if (alertMessage !== null && alertMessage !== "") {
        UmbClientMgr.showMessage('save', 'Saved', alertMessage);
    }
    
    // Init boostrap datetime-pcker and other stuffs
    Uvendia.init();

    $('.tagsInput').tagsInput({
        'height': '70px',
        'width': '100%',
        'interactive': true,
        'defaultText': 'Add a tag',
        'delimiter': [','],   // Or a string with a single delimiter. Ex: ';'
        'removeWithBackspace': true,
        'placeholderColor': '#666666'
    });

    // Handles extended checkboxes
    $(document).on('change', 'input.extended-checkbox, input.check-box', function () {        
        var val = $(this).is(':checked');
        $(this).val(val);
        $(this).parent().find('input[type="hidden"]').val(val);
    });

    $(document).on('click', '.linkProduct', function (event) {
        var hdn = $('#TreePath');
        var treePath = 'store;store_';
        if (hdn.length) {
            treePath = hdn.val();
        }
        
        UmbClientMgr.contentFrame('/Backoffice/Uvendia/Product/CreateEdit/' + $(this).data("product-id") + '?treePath=' + treePath + '&categoryId=' + $(this).data("category-id"));
        Uvendia.closeModal(null);        
    });

    $('.linkOrder').click(function () {
        UmbClientMgr.contentFrame('/Backoffice/Uvendia/Order/CreateEdit/' + $(this).data("order-id") + '?statusId=' + $('#OrderStatusId').val() + '&treePath=' + $('#TreePath').val());
    });
    $(document).on('click', '.linkEvent', function () {
        UmbClientMgr.contentFrame('/Backoffice/Uvendia/Event/CreateEdit/' + $(this).data("event-id") + '/?treePath=' + $('#TreePath').val());
    });

    // Load the extended properties
    var extendedContainer = $('#extended');
    if (extendedContainer.length) {        
        $('#ddlDefinitions').change(function () {
            Uvendia.loadExtendedControls($(this).val(), extendedContainer.data('item-id'), extendedContainer);
        });
    }

    $("#btnAddNewVariant").on("click", function (event) {

        var container = $("div#container_variants");
        var sub = container.find('div.variants');
        var variantCount = sub.length;
        //alert(variantCount);

        var clonedElement = sub.eq(0).clone();
        var els = clonedElement.find("input, select, a, textarea");
        clonedElement.attr('data-index', variantCount);
        els.each(function () {
            
            if ($(this).is('a')) {
                $(this).attr('data-id', 0);
                $(this).attr('data-index', variantCount);
            }
            else {
                var newName = $(this).attr("name").replace("[0]", "[" + variantCount + "]");
                $(this).attr("name", newName);
            }
            //$(this).attr("id", newId);

            // Clear input
            if ($(this).is('input[type="text"]')) {
                $(this).attr('value', '');
                $(this).val('');
            }
            else if ($(this).attr('id') === 'Variants_Index') {
                $(this).val(variantCount);
            }
            else if ($(this).is('input[type="hidden"]') && $(this).data('clearable') !== false) {
                $(this).removeAttr('value');
            }
            else if ($(this).is('select')) {
                $(this).val($(this).find("option:first").val());
            }
            
        });

        container.append(clonedElement);
        return false;
    });

    // Add new ticket type, stocks and payment method settings
    $(document).on("click", "#btnAddNewTicketType, #btnAddPaymentMethodSetting, #btnAddNewStock", function (event) {

        var tableName;
        if ($(this).attr('id') === 'btnAddNewTicketType') {
            tableName = 'tblTicketTypes';
        }
        else if ($(this).attr('id') === 'btnAddPaymentMethodSetting') {
            tableName = "tblPaymentMethodSetting";
        }
        else {
            tableName = "tblStocks";
        }

        var table = $('table#' + tableName);
        var rowCount = table.find('tbody > tr').length;
        var container = table.find('tbody');
        //alert(rowCount);

        var clonedElement = table.find("tbody > tr").eq(0).clone();
        var els = clonedElement.find("input, select, a, textarea, td");
        clonedElement.attr('data-index', rowCount);
        els.each(function () {   
            
            if ($(this).is('a')) {
                $(this).attr('data-id', 0);
                $(this).attr('data-index', rowCount);
            }
            else if ($(this).is('td') /*&& table.attr('id') === 'tblStocks'*/) {
                if ($(this).data('clearable') === true) {
                    $(this).html('');
                }
                else if ($(this).data('clearable') === false) {
                    var subEls = $(this).find('input, select');
                    subEls.each(function () { // attr to sub elements
                        $(this).attr('data-clearable', 'false');
                    });
                }
            }
            else if (typeof $(this).data('readonlyname') === 'undefined' || $(this).data('readonlyname') === null || $(this).data('readonlyname') === false) {
                var oldName = $(this).attr("name");
                var newName = Uvendia.replaceLast(oldName, "[0]", "[" + rowCount + "]");
                $(this).attr("name", newName);
            }

            if ($(this).is('input[type="text"]')) {
                $(this).attr('value', '');
                $(this).val('');
            }
            else if ($(this).is('input[type="number"]')) {
                //$(this).attr('value', '0');
                $(this).val('0');
                $(this).removeAttr('readonly');
            }
            else if ($(this).attr('id') === 'Event_TicketsSale_Index') {
                $(this).val(rowCount);
            }
            else if ($(this).attr('id') === 'PaymentMethod_Settings_Index') {
                $(this).val(rowCount);
            }
            else if (typeof $(this).attr('id') !== 'undefined' && $(this).attr('id') !== null && $(this).attr('id').includes('Inventories_Index')) {
                $(this).val(rowCount);
            }
            else if ($(this).is('input[type="hidden"]') && $(this).data('number') === true) {
                $(this).val('0');
            }
            else if ($(this).is('input[type="hidden"]') && $(this).data('clearable') !== false) {
                $(this).removeAttr('value');
            }
            else if ($(this).is('select')) {
                $(this).val($(this).find("option:first").val());
            }
        });

        var deleteRow;
        var rowId = Uvendia.newGuid();
        clonedElement.attr('id', rowId); // Set row id

        if (tableName === 'tblTicketTypes') {
            deleteRow = clonedElement.find('td.deleteRow');
            deleteRow.html('');
            deleteRow.append('<a title="Delete ticket type" data-rowid="' + rowId + '" data-id="0" data-controller="TicketSale" class="deleteTicketSale"><i class="glyphicon glyphicon-trash"></i></a>');
            container.append(clonedElement);
        }
        else if (tableName === 'tblStocks') {
            
            deleteRow = clonedElement.find('td.deleteRow');
            deleteRow.html('');
            deleteRow.append('<a title="Delete Inventory" data-id="0" data-rowid="' + rowId + '" data-controller="Product" class="deleteInventory"><i class="glyphicon glyphicon-trash"></i></a>');
            clonedElement.appendTo(table);

            // Save the newly generated stock (html) row
            var hiddenNewInventories = table.parent().find('input.new-stock');
            var currentVal = hiddenNewInventories.val();
            hiddenNewInventories.val(currentVal + clonedElement[0].outerHTML);
        }
        else {
            deleteRow = clonedElement.find('td.deleteRow');
            deleteRow.html('');
            deleteRow.append('<a title="Delete setting" data-rowid="' + rowId + '" data-id="0" class="deletePaymentMethodSetting"><i class="glyphicon glyphicon-trash"></i></a>');
            clonedElement.appendTo(table);
        }

        // Init boostrap datetime-pcker
        Uvendia.init();        
        return false;
    });

    $(document).on("click", "a.openVariantDescription", function (event) {
        Uvendia.showModal('/Umbraco/Backoffice/Uvendia/Product/ShowModal/?id=' + $(this).data('id') + '&type=translation' + '&index=' + $(this).data('index') + '&t=' + $(this).data('modifiedon'),
            'Descriptions', 'medium',
            Uvendia.onCloseVariantModal);        
    });
    $(document).on("click", "a.openVariantPrices", function (event) {
        Uvendia.showModal('/Umbraco/Backoffice/Uvendia/Product/ShowModal/?id=' + $(this).data('id') + '&type=price' + '&index=' + $(this).data('index') + '&t=' + $(this).data('modifiedon'),
            'Prices', 'small',
            Uvendia.onCloseVariantModal);
    });
    $(document).on('click', 'a.delete', function (event) {
        var controller = $(this).data('controller');
        var title = $(this).attr('title');
        var method = '';
        if (typeof $(this).data('method') !== 'undefined')
            method = $(this).data('method');
        var rowId = '';
        if (typeof $(this).data('rowid') !== 'undefined')
            rowId = $(this).data('rowid');

        var row_index = $(this).closest('tr').index();
        Uvendia.showModal('/Umbraco/Backoffice/Uvendia/Base/Delete/?id=' + $(this).data('id') + '&title=' + title + '&cntrl=' + controller + '&method=' + method + '&index=' + row_index + '&rowId=' + rowId,
            title, 'small',
            Uvendia.onCloseDeleteModal);
        
    });

    $(document).on("click", "a.deleteVariant", function (event) {

        // Delete from database
        var title = $(this).attr('title');

        var index = $(this).data('index');
        Uvendia.showModal('/Umbraco/Backoffice/Uvendia/Base/Delete/?id=' + $(this).data('id') + '&title=' + title + '&cntrl=Product&index=' + index,
            title, 'small',
            Uvendia.onCloseDeleteProductVariantModal);        
    });

    $(document).on("click", "a.deleteTicketSale", function (event) {

        // Delete from database
        var title = $(this).attr('title');
        var rowId = '';
        if (typeof $(this).data('rowid') !== 'undefined')
            rowId = $(this).data('rowid');

        var row_index = $(this).closest('tr').index();
        Uvendia.showModal('/Umbraco/Backoffice/Uvendia/Base/Delete/?id=' + $(this).data('id') + '&title=' + title + '&cntrl=TicketSale&index=' + row_index + '&rowId='+ rowId,
            title,
            "small",
            Uvendia.onCloseDeleteModal);        
    });

    $(document).on("click", "a.deleteInventory", function (event) {        
        // Delete from database
        var title = $(this).attr('title');
        var rowId = '';
        if (typeof $(this).data('rowid') !== 'undefined')
            rowId = $(this).data('rowid');

        var row_index = $(this).closest('tr').index();
        Uvendia.showModal('/Umbraco/Backoffice/Uvendia/Base/Delete/?id=' + $(this).data('id') + '&title=' + title + '&cntrl=Product&index=' + row_index + '&method=DeleteInventory&rowId=' + rowId,
            title,
            "small",
            Uvendia.onCloseDeleteModal);                
    });

    $("table#tblPaymentMethodSetting").on("click", "a.deletePaymentMethodSetting", function (event) {

        $(this).closest('tr').remove();                
    });

    $(document).on('click', "button.save-and-close-translations, button.save-and-close-prices", function (event) {
        var els = $('#tabs').find("input, select, textarea");
        var index = $(this).data('index');
        var array = [];

        els.each(function () {
            if ($(this).attr('name') !== null && $(this).data('copyable') !== false) {
                var obj = { id: $(this).attr('id'), name: $(this).attr('name'), value: $(this).val() };
                array.push(obj);
            }
        });
        var outVal = { index: index, array: array };

        Uvendia.closeModal(outVal);       
    });

    $(document).on("click", "button.closemodal", function (event) {
        Uvendia.closeModal(null);
    });

    $(document).on('click', 'button.saveCloseCloudinaryModal', function () {
        var hdn = $('#hdnCloudinaryJson');
        if (hdn.length) {
            var json = JSON.parse(hdn.val());
            json.propValRef = $('#PropertyValueReference').val();
            Uvendia.closeModal(json);
        }
    });

    // Delete & Close item
    $(document).on('click', 'button.delete-and-close',  function (event) {
        var item =
        {
            controller: $(this).data('contr'),
            id: $(this).data('id'),
            method: $(this).data('method'),
            index: $(this).data('index'),
            rowId: $(this).data('rowid')
        };
        
        Uvendia.closeModal(item);
    });

    // Upload or choose a cloudinary image
    $(document).on('click', 'a.cloudinary-choose', function (event) {
        var folders = '';
        if ($('#ImageFolders').length && $('#ImageFolders').val() !== null) folders = $('#ImageFolders').val();
        folders = folders.replace(' ', '').toLowerCase();
        var propValRef = '';
        var hidden = $(this).parent().find('input[type="hidden"].cloudinary-publicId');
        if (hidden.length) propValRef = hidden.attr('name');
        var fileType = $(this).data('cloudinary-type');
        Uvendia.showModal('/Umbraco/Backoffice/Uvendia/Base/CloudinaryFiles?folders=' + folders + '&propValueRef=' + propValRef + '&fileType=' + fileType,
            'Cloudinary Media Selection', 'medium',
            Uvendia.onCloseCloudinaryFilesModal);
        
    });

    // remove cloudinary image from item
    $(document).on('click', 'a.cloudinary-image-delete', function (event) {
        var parent = $(this).parent().parent();
        parent.find('input.cloudinary-publicId').val('');
        parent.find('a.cloudinary-choose').show();
        $(this).parent().hide();        
    });

    // Image selector
    $(document).on('change', 'input.image_selector', function (event) {
        var htmlTag = $(this).parent().parent().find('div.containerHtmlTag').html();
        var data = { imageId: $(this).data('image-id'), url: $(this).data('image-url'), fileType: $('div.media-container').data('cloudinary-type'), htmlTag: htmlTag };

        var hiddenInputSucceedJson = $('#hdnCloudinaryJson');
        if (!hiddenInputSucceedJson.length) {
            hiddenInputSucceedJson = $('<input/>', {
                type: "hidden",
                id: "hdnCloudinaryJson"
            });
            $(this).parents('#tabs-select').append(hiddenInputSucceedJson);
        }
        hiddenInputSucceedJson.val(JSON.stringify(data));
        Uvendia.checkImageSelector($(this).attr('name'));        
    });

    $(document).on('click', 'button#btnLoadMoreCloudinaryFiles', function (event) {
        var pageIndexInput = $('input#PageIndex');
        var pageIndex = parseInt(pageIndexInput.val());
        var pageSize = parseInt($('input#PageSize').val());
        var totalRows = parseInt($('input#TotalRows').val());

        if (((pageIndex - 1) * pageSize) < totalRows) {

            var fileType = $(this).data('filetype');
            var container = $('div#container-cloudinary-files');

            $.ajax({
                type: "GET",
                url: '/Umbraco/Backoffice/Uvendia/Base/LoadMoreCloudinaryFiles/?fileType=' + fileType + '&pageIndex=' + pageIndex + '&pageSize=' + pageSize,
                cache: false,
                dataType: 'html',
                error: function (XMLHttpRequest, textStatus, errorThrown) {

                },
                success: function (data, textStatus, XMLHttpRequest) {
                    console.log(data);
                    container.append($(data));
                },
                complete: function (XMLHttpRequest, textStatus) {
                    pageIndex++;
                    pageIndexInput.val(pageIndex);
                }
            });
        }
        else {
            $(this).attr('disabled', 'disabled');
        }
    });

    // Retour order product checkbox changed
    $('div#container-orderdetails').on('change', 'input.product-retoured', function () {

        var row = $(this).closest('tr');
        if ($(this).prop("checked")) {
            row.css("background-color", "#ffcccc");
        }
        else {
            row.css("background-color", "");
        }

        Uvendia.calculationNeedRefresh();
    });
        
    // Add new product order detail
    $('#btnAddOrderDetail').click(function (event) {
        Uvendia.showModal('/Umbraco/Backoffice/Uvendia/Search/Index?addorderdetail=true',
            'Product Order detail', 'medium',
            Uvendia.onCloseAddOrderDetail);        
    });

    // Add new ticket order detail
    $('#btnAddTicketOrderDetail').click(function (event) {
        Uvendia.showModal('/Umbraco/Backoffice/Uvendia/Event/Search?addorderdetail=true',
            'Ticket Order detail', 'medium',
            Uvendia.onCloseAddOrderDetail);        
    });

    // Save & Close search model
    $(document).on('click', 'button.save-and-close-search', function (event) {
        var array = [];
        $('input.product-select, input.event-select').each(function () {
            if ($(this).prop("checked")) {

                var ticketHasCancellationInsurance = false;
                var row = $(this).closest('tr');
                var chb = row.find('.cancellation-insurance-select');
                if (chb.length) {
                    ticketHasCancellationInsurance = chb.is(':checked');
                }
                var retoured = false;
                var chbRetoured = row.find('.retoured-select');
                if (chbRetoured.length) {
                    retoured = chbRetoured.is(':checked');
                }

                var obj = { id: $(this).data('id'), name: $(this).data('name'), type: $(this).data('type'), ticketHasCancellationInsurance: ticketHasCancellationInsurance, retoured: retoured };
                array.push(obj);
            }
        });
        Uvendia.closeModal(array);
    });
    
    // Change total / vat Order
    $(document).on('change', 'select.ddlUnitPrice, input.quantity', function () {

        Uvendia.calculatePrices($(this));
    });
    $('div#container-orderdetails').on('click', 'a#btnRefresh', function () {
        Uvendia.refreshOrderDetails();
    });

    $('div#container-orderdetails').on('click', 'tr.clickable', function () {
        $(this).find('i').toggleClass('glyphicon-plus glyphicon-minus');
    });

    // Inventory
    Uvendia.showOrHideInventory();
    $(document).on('change', '#chbHasStockIndication', function () {
        Uvendia.showOrHideInventory();
    });

    Uvendia.disableStocks();

});

(function ($) {

    Uvendia = {

        init: function () {
            
            $('.datetime').datetimepicker({
                viewMode: 'years',
                format: 'DD MMMM YYYY'
            });

            $('div.datetimepicker').datetimepicker({
                viewMode: 'years',
                format: 'DD MMMM YYYY HH:mm',
                debug: false
            });
            
            tinymce.init({
                selector: 'textarea.editor',
                plugins: ["charmap", "table", "hr", "anchor", "code", "autolink", "link", "lists", "paste", "print", "autoresize", "emoticons", "searchreplace", "fullscreen", "autoresize"],
                textcolor_cols: "5",
                textcolor_rows: "4",
                toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist checklist | forecolor backcolor casechange permanentpen formatpainter removeformat | pagebreak | emoticons | fullscreen  preview save print | charmap',
                emoticons_append: {                    
                    robot: {
                        keywords: ["computer", "machine", "bot"],
                        char: "🤖",
                        category: "people"
                    },
                    dog: {
                        keywords: ["animal", "friend", "nature", "woof", "puppy", "pet", "faithful"],
                        char: "🐶",
                        category: "animals_and_nature"
                    }
                },
                max_height: 500,
                resize: true,
                setup: function (editor) {
                    editor.on('change', function () {
                        tinymce.triggerSave();
                    });
                }
            });                      
          

            $('[data-toggle="tooltip"]').tooltip();
        },
        selectTab: function (tabindex) {            
            $('#tabs .nav-tabs li:eq(' + tabindex + ') a').tab('show');                        
            $("#tabs").tabs("option", "active", tabindex);
        },
        cleanHTML: function (text) {
            return text.replace(/<[^>]*>?/gm, '');
        },
        loadExtendedControls: function (defId, itemId, container) {
            container.html('');
            var prefix = container.data('prefix');
            var section = container.data('section');
            $.ajax({
                type: "GET",
                url: '/Umbraco/Backoffice/Uvendia/Base/RenderExtendedProperties/?defId=' + defId + '&itemId=' + itemId + '&prefix=' + prefix + '&section=' + section,
                cache: false,
                dataType: 'html',
                error: function (XMLHttpRequest, textStatus, errorThrown) {

                },
                success: function (data, textStatus, XMLHttpRequest) {
                    container.html(data);

                },
                complete: function (XMLHttpRequest, textStatus) {
                    // Init the fancy stuffs
                    Uvendia.init();
                }
            });            
        },        
        showModal: function (url, title, size = 'small', onClose = null) {
            UmbClientMgr.openModalWindow(url, title, size, onClose);
        },
        closeModal: function (data) {
            UmbClientMgr.closeModalWindow(data);
        },
        onCloseDeleteModal: function (data) {

            if (data.outVal !== null) {
                var item = data.outVal;
                //console.log('item ', item.id);

                if (typeof item.method === 'undefined' || item.method === 'undefined' || item.method === null || item.method === '') {
                    item.method = 'Delete' + item.controller;
                }

                $.ajax({
                    type: "GET",
                    url: '/Umbraco/Backoffice/Uvendia/' + item.controller + '/' + item.method + '/?id=' + item.id,
                    cache: false,
                    dataType: 'json',
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        var msg = $(XMLHttpRequest.responseText)[1].innerText;
                        UmbClientMgr.showMessage('error', 'Error', msg);
                    },
                    success: function (data, textStatus, XMLHttpRequest) {
                        //console.log(item);
                        UmbClientMgr.showMessage('save', 'Deleted', data);
                        
                        if ($('#' + item.rowId).length) {
                            $('#' + item.rowId).remove();
                        }
                        else {
                            var tr = $('tr[data-index=' + item.index + ']');

                            if (tr.length) {
                                tr.remove();
                            }                            
                        }

                        if (item.method === 'DeleteInventory') {
                            Uvendia.disableStocks(); // Refresh disabled elements
                        }

                        // Save (item) row-id                        
                        var deleted_container = $('input[type="hidden"].deleted-stocks');
                        if (deleted_container.length) {
                            var deletedItems = deleted_container.val();
                            if (deletedItems !== null && deletedItems !== '') {
                                deleted_container.val(deletedItems + ';' + item.rowId);
                            }
                            else {
                                deleted_container.val(item.rowId);
                            }
                        }

                        if (item.controller === 'TicketOrderDetail' || item.controller === 'ProductOrderDetail') {
                            Uvendia.refreshOrderDetails();
                        }
                    },
                    complete: function (XMLHttpRequest, textStatus) {

                    }
                });
            }
        },
        onCloseDeleteProductVariantModal: function (data) {
            if (data.outVal !== null) {
                var item = data.outVal;
                var container = $("div#container_variants");

                $.ajax({
                    type: "GET",
                    url: '/Umbraco/Backoffice/Uvendia/' + item.controller + '/Delete' + item.controller + '/?id=' + item.id + '&index=' + item.index,
                    cache: false,
                    dataType: 'json',
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        var msg = $(XMLHttpRequest.responseText)[1].innerText;
                        UmbClientMgr.showMessage('error', 'Error', msg);
                    },
                    success: function (data, textStatus, XMLHttpRequest) {

                        UmbClientMgr.showMessage('save', 'Deleted', data);

                        // Clear product variant from page
                        var variantCount = container.find('div.variants').length;
                        var deleteLink = container.find('a[data-index=' + item.index + ']');

                        if (variantCount > 1)
                            deleteLink.closest("div.variants").remove();
                        else {
                            var els = container.find("input, select");
                            els.each(function () {

                                $(this).val('');

                            });
                        }
                    },
                    complete: function (XMLHttpRequest, textStatus) {

                    }
                });
            }
        },
        onCloseVariantModal: function (data) {
            if (data.outVal !== null) {
                var container = $('div#container_variants');//$('div.variants[data-index="' + data.outVal.index + '"]');

                data.outVal.array.forEach(function (t) {
                    
                    var hidden = container.find('input[name="' + t.name + '"]');
                    if (hidden.length)
                        hidden.val(t.value);
                    else {
                        var hiddenInput = $('<input/>', {
                            type: "hidden",
                            name: t.name,
                            id: t.id,
                            value: t.value
                        });
                        // Add the hidden input
                        container.append(hiddenInput);
                    }                    
                });
            }
        },
        onCloseCloudinaryFilesModal: function (data) {
            if (data.outVal !== null) {
                var hiddenProp = $('input[name="' + data.outVal.propValRef + '"]');
                var container = hiddenProp.parent();
                var link = container.find('a.cloudinary-choose');

                // Store the image publicId
                hiddenProp.val(data.outVal.imageId);

                // Hide the link
                link.hide();

                // Show image
                if (data.outVal.fileType === "image") {
                    var img = container.find('img.cloudinary-image');
                    img.attr("src", data.outVal.url);
                    img.attr('title', data.outVal.imageId);
                }
                else if (data.outVal.fileType === "video") {
                    var videoContainer = container.find('div.cloudinary-video');
                    videoContainer.html(data.outVal.htmlTag);
                }
                else {
                    var downloadLink = container.find('a#downloadLink');
                    downloadLink.attr('href', data.outVal.url);
                    var fileContainer = container.find('div.cloudinary-file');
                    fileContainer.html(data.outVal.htmlTag);
                }
                //var hiddenPublicId = container.find('.cloudinary-publicId');
                //hiddenPublicId.val(data.outVal.imageId);

                container.find('div.cloudinary-image-container').show();
            }
        },
        onCloseAddOrderDetail: function (data) {
            if (data.outVal !== null) {

                // { id: $(this).data('id'), name: $(this).data('name'), type: $(this).data('type') };
                var container = $('#container-orderdetails');
                var orderId = $('#hdnOrderId').val();
                var paymentMethodId = $('#ddlPaymentMethod').val();
                var shippingMethodId = $('#ddlShippingMethod').val();
                var orderInfo = {
                    orderId,
                    paymentMethodId,
                    shippingMethodId
                };

                $.ajax({
                    type: "POST",
                    url: '/Umbraco/Backoffice/Uvendia/Order/OrderDetails',
                    cache: false,
                    async: false,
                    dataType: 'html',
                    data: { details: data.outVal, orderInfo: orderInfo },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {

                    },
                    success: function (data, textStatus, XMLHttpRequest) {
                        if ($('#tblOrderDetails').length) {
                            var currentRows = $('#tblOrderDetails > tbody > tr');
                            var currentRowsCount = currentRows.length - 1;

                            // Get the new rows
                            var html = $(data);
                            var rows = html.filter('#tblOrderDetails').find('tbody > tr');

                            // add new rows to table
                            rows.each(function () {
                                var isEvent = ($(this).data('type') === 'event');

                                var existingRow = $('#tblOrderDetails > tbody > tr[data-id=' + $(this).data('id') + '][data-type=' + $(this).data('type') + '][data-retoured=' + $(this).data('retoured') + ']');
                                if (existingRow.length) {
                                    var txt = existingRow.find('.quantity');
                                    if (txt.val() === '') txt.val('0');

                                    var quantity = parseInt(txt.val());

                                    // Update new quantity.
                                    quantity = quantity + 1;
                                    txt.val(quantity);
                                    // Update totals
                                    if (!isEvent) {
                                        Uvendia.calculatePrices(txt);
                                    }
                                }
                                else {   
                                    var tableBody = $('#tblOrderDetails > tbody');
                                    // Add new row
                                    var newRow = $(this).clone();
                                    var elements = null;
                                    var detailRowCount = 0;

                                    if (!isEvent) {
                                        // find the rows for product
                                        detailRowCount = tableBody.find('tr[data-type="product"][data-entity="true"]').length;
                                    }
                                    else {
                                        detailRowCount = tableBody.find('tr[data-type="event"][data-entity="true"]').length;
                                    }

                                    // Get the input elements from the new-row
                                    elements = newRow.find("input, select");
                                    // Change the mvc array index for new elements
                                    if (elements !== null) {
                                        elements.each(function () {
                                            if ($(this).hasClass('index')) {
                                                $(this).val(detailRowCount);
                                            }
                                            else {                                                                                                
                                                var newName = $(this).attr("name").replace(/\[.*?\]\s?/g, "[" + detailRowCount + "]");                                                
                                                $(this).attr("name", newName);
                                            }
                                        });
                                    }
                                                                        
                                    if (isEvent) {
                                        
                                        var mainRow = tableBody.find("tr.clickable[data-id='" + newRow.data("item-id") + "'][data-type='event']");
                                            
                                        mainRow.after(newRow);
                                
                                        if (mainRow.length) {  
                                            // Refresh prices
                                            var txtQuantity = mainRow.find('input.quantity');
                                            Uvendia.calculatePrices(txtQuantity);
                                        }
                                        else {
                                            tableBody.append(newRow);
                                            
                                        }
                                        $('.collapse, .clickable').collapse('hide');
                                        tableBody.find('i.glyphicon-minus').toggleClass('glyphicon-minus glyphicon-plus');

                                        //mainRow.collapse('show');   
                                        //newRow.collapse();  
                                    }
                                    else {
                                        tableBody.append(newRow);
                                    }
                                }                                
                            });

                            Uvendia.calculationNeedRefresh();
                        }
                        else {
                            container.html(data);
                        }
                    },
                    complete: function (XMLHttpRequest, textStatus) {

                    }
                });
            }
        },
        checkImageSelector: function (name) {
            $('input.image_selector').each(function () {
                if ($(this).attr('name') !== name) {
                    $(this).prop('checked', false);
                }
                if ($(this).is(':checked')) {
                    $('button.saveCloseCloudinaryModal').removeAttr('disabled');
                }
            });
        },
        calculatePrices: function (elem) {

            var row = elem.closest('tr');
            var td_discount = row.find('td.discount');
            var td_vat = row.find('td.vat');
            var td_total = row.find('td.total');

            var quantity = parseInt(row.find('input.quantity').val());
            var itemId = parseInt(row.find('input.item-id').val());
            var type = row.data('type');

            var table = row.closest('table');            
            var totalCancellationInsured = table.find("tr[data-item-id='" + itemId + "'][data-cancellation-insured='True']").length;
                        
            var priceId = 0;
            var ddlUnitPrice = row.find('select.ddlUnitPrice');
            if (ddlUnitPrice.length)
                priceId = parseInt(ddlUnitPrice.val());

            var retoured = false;
            var hdnRetoured = row.find('input.retoured');
            if (hdnRetoured.length && hdnRetoured.val() !== null)
                retoured = (hdnRetoured.val().toLowerCase() === 'true');

            var detail = { itemId: itemId, quantity: quantity, priceId: priceId, type: type, totalCancellationInsured: totalCancellationInsured, retoured: retoured };
            
            $.ajax({
                type: "POST",
                url: '/Umbraco/Backoffice/Uvendia/Order/Calculate/',
                cache: false,
                dataType: 'json',
                async: false,
                data: { detail: detail },
                error: function (XMLHttpRequest, textStatus, errorThrown) {

                },
                success: function (data, textStatus, XMLHttpRequest) {

                    td_discount.html(data.discount.toFixed(2));
                    td_vat.html(data.vat.toFixed(2));
                    td_total.html(data.total.toFixed(2));
                },
                complete: function (XMLHttpRequest, textStatus) {
                    Uvendia.calculationNeedRefresh();
                }
            });
        },
        removeRow: function (e) {
            e.parentNode.parentNode.parentNode.removeChild(e.parentNode.parentNode);
        },        
        calculationNeedRefresh: function () {
            if ($('#tblOrderDetails > tfoot').length) {
                $('#tblOrderDetails > tfoot > tr > td:last-child').css("text-decoration", "line-through");
                $('#btnRefresh').css("background-color", "#ffe066");
                $('button.save-needed').attr('disabled', 'disabled');
            }
        },
        refreshOrderDetails: function () {
            var currentRows = $('#tblOrderDetails > tbody > tr[data-entity="true"]');
            var array = [];
            currentRows.each(function () {

                var detailId = $(this).find('.detailId').val();
                var priceId = null;                
                var retoured = false;   
                var ticketHasCancellationInsurance = false;

                var hdnPriceId = $(this).find('.hdnPriceId');
                var hdnRetoured = $(this).find('.retoured');
                var hdnCancellationInsured = $(this).find('.hdnCancellationInsured');

                if (hdnPriceId.length) {
                    priceId = hdnPriceId.val();
                }               

                if (hdnRetoured.length) {
                    retoured = hdnRetoured.val().toLowerCase() === 'false' ? false : true;
                }

                if (hdnCancellationInsured.length) {
                    ticketHasCancellationInsurance = hdnCancellationInsured.val().toLowerCase() === 'false' ? false : true;
                }

                var quantity = $(this).find('.quantity').val();

                var obj = {
                    detailId: detailId,
                    id: $(this).data('item-id'),
                    name: $(this).data('name'),
                    type: $(this).data('type'),
                    priceId: priceId,
                    quantity: quantity,
                    retoured: retoured,
                    ticketHasCancellationInsurance: ticketHasCancellationInsurance
                };

                array.push(obj);
            });
                        
            if (array.length > 0) {
                var all_priceIds = array.map(function (a) { return a.priceId; });
                if (!Uvendia.containsMultipleCurrencies(all_priceIds)) {
                    var data = { outVal: array };
                    $('div#container-orderdetails').html('');
                    Uvendia.onCloseAddOrderDetail(data);
                }
                else {
                    UmbClientMgr.showMessage('error', 'Error', 'Multiple currencies detected');
                }
            }            
        },
        containsMultipleCurrencies: function (priceIds) {
            var hasMultiple = false;
            $.ajax({
                type: "POST",
                url: '/Umbraco/Backoffice/Uvendia/Order/checkmultiplecurrencies',
                cache: false,
                async: false,
                dataType: 'json',
                data: { priceIds },
                error: function (XMLHttpRequest, textStatus, errorThrown) {

                },
                success: function (data, textStatus, XMLHttpRequest) {
                    hasMultiple = data.hasMultipleCurrencies;
                },
                complete: function (XMLHttpRequest, textStatus) {

                }
            });
            return hasMultiple;
        },
        deleteCookie: function (cname) {
            var d = new Date(); //Create an date object
            d.setTime(d.getTime() - (1000 * 60 * 60 * 24)); //Set the time to the past. 1000 milliseonds = 1 second
            var expires = "expires=" + d.toGMTString(); //Compose the expirartion date
            window.document.cookie = cname + "=" + "; " + expires;//Set the cookie with name and the expiration date
        },
        closeLoadingSpinner: function () {
            $("#loading").fadeOut();
        },
        showOrHideInventory: function () {
            var body = $('div.uvendia-body'); 
            var chb = body.find('#chbHasStockIndication');

            if (chb.length) {
                var container = body.find('#container-stocks');
                if (chb.prop('checked')) {
                    container.show();
                }
                else
                    container.hide();
            }
        },
        replaceLast: function (text, what, replacement) {
            if (typeof text !== 'undefined' && text !== null && text.indexOf(what) !== -1) {
                var pcs = text.split(what);
                var lastPc = pcs.pop();
                return pcs.join(what) + replacement + lastPc;
            }
            return text;
        },
        newGuid: function () {
            var dt = new Date().getTime();
            var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = (dt + Math.random() * 16) % 16 | 0;
                dt = Math.floor(dt / 16);
                return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
            });
            return uuid;
        },
        disableStocks: function () {
            // Stock count already filled is 'readonly'
            var stocks = $('table#tblStocks input.stock');
            
            if (stocks.length) {
                $(stocks[stocks.length - 1]).removeAttr('readonly');
                for (var i = 0; i < stocks.length; i++) {                    
                    if (i < stocks.length - 1) {
                        $(stocks[i]).attr('readonly', 'readonly');
                    }                    
                }
            }
        }
    };
})(jQuery);