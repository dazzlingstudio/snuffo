var app = angular.module("umbraco");
app.controller("Uvendia.ProductSelectorController",
    function ($scope, assetsService) {
        assetsService
            .load([
                "~/Umbraco/Uvendia/Content/select2/js/select2.min.js"
            ])
            .then(function () {   
                var productSelect2 = $('.select2-js-data-ajax');

                if ($scope.model.value !== null && $scope.model.value !== '') {
                    // Load selected values
                    
                    var split = $scope.model.value.split(',');
                    var selected = split.join("&id=");
                    
                    $.ajax({
                        type: "GET",
                        url: '/umbraco/backoffice/uvendiaplugin/CatalogApi/GetProductsById/?id=' + selected,
                        cache: false,
                        dataType: 'json',
                        error: function (XMLHttpRequest, textStatus, errorThrown) {

                        },
                        success: function (data, textStatus, XMLHttpRequest) {                            
                            // create the option and append to Select2
                            for (var i = 0; i < data.length; i++) {

                                var txt = data[i].name + ' (' + (data[i].variantSku || data[i].sku) + ')';
                                var option = new Option(txt, data[i].id, true, true);
                                productSelect2.append(option).trigger('change');
                            }
                            productSelect2.trigger({
                                type: 'select2:select',
                                params: {
                                    data: data
                                }
                            });
                        },
                        complete: function (XMLHttpRequest, textStatus) {

                        }
                    });
                }                

                productSelect2.select2({                    
                    ajax: {
                        url: "/umbraco/backoffice/uvendiaplugin/CatalogApi/GetProductsByQuery/",
                        dataType: 'json',
                        delay: 250,
                        width: 'resolve',
                        allowClear: true,
                        data: function (params) {
                            return {
                                q: params.term
                            };
                        },
                        processResults: function (data, params) {
                            // parse the results into the format expected by Select2
                            // since we are using custom formatting functions we do not need to
                            // alter the remote JSON data, except to indicate that infinite
                            // scrolling can be used
                            params.page = params.page || 1;

                            return {
                                results: data,
                                pagination: {
                                    more: (params.page * 30) < data.length
                                }
                            };
                        },
                        cache: true
                    },
                    placeholder: 'Search for product(s) to select',
                    minimumInputLength: 3,
                    escapeMarkup: function (markup) { return markup; }, // let our custom formatter work
                    templateResult: formatProduct,
                    templateSelection: formatProductSelection
                }).on("change", function (e)
                    {
                        save(e);
                    });

                function formatProduct(product) {
                    if (product.loading) {
                        return product.name;
                    }

                    var markup = 
                        "<div class='select2-result-product__title'>" + product.name + "</div>";

                    if (!product.variantSku) {
                        markup += "<div class='select2-result-product__description'>Sku: " + product.sku + "</div>";
                    }
                    else {
                        markup += "<div class='select2-result-product__description'>Sku: " + product.sku + ", VariantSku: " + product.variantSku + " <i>(variant)</i></div>";
                    }
                    
                    return markup;
                }

                function formatProductSelection(product) {
                    
                    if (product.name) {
                        var txt = product.name + ' (' + (product.variantSku || product.sku) + ')';
                        return txt;
                    }
                    else
                        return product.text;
                }

                function save(e) {
                    var array = $(e.currentTarget).val();
                    if (array) {
                        $scope.model.value = array.join(",");
                    }
                    else
                        $scope.model.value = null;

                    //console.log('model.value', $scope.model.value);
                }
                
            });

        // load the separate css for the editor to avoid it blocking our js loading
        assetsService.loadCss("~/Umbraco/Uvendia/Content/select2/css/select2.min.css");
        assetsService.loadCss("~/Umbraco/Uvendia/Content/umbraco-datatypes.css");

    });