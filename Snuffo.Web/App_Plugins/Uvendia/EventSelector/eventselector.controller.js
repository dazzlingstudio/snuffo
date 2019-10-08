var app = angular.module("umbraco");
app.controller("Uvendia.EventSelectorController",
    function ($scope, assetsService) {
        assetsService
            .load([
                "~/Umbraco/Uvendia/Content/select2/js/select2.min.js"
            ])
            .then(function () {   
                var eventSelect2 = $('.select2-js-event-data-ajax');

                if ($scope.model.value !== null && $scope.model.value !== '') {
                    // Load selected values
                    
                    var split = $scope.model.value.split(',');
                    var selected = split.join("&id=");
                    
                    $.ajax({
                        type: "GET",
                        url: '/umbraco/backoffice/uvendiaplugin/EventApi/GetEventsById/?id=' + selected,
                        cache: false,
                        dataType: 'json',
                        error: function (XMLHttpRequest, textStatus, errorThrown) {

                        },
                        success: function (data, textStatus, XMLHttpRequest) {                            
                            // create the option and append to Select2
                            for (var i = 0; i < data.length; i++) {

                                var txt = data[i].name;
                                if (data[i].startDate)
                                    txt += " (" + data[i].startDate + " - " + data[i].endDate + ")";

                                var option = new Option(txt, data[i].id, true, true);
                                eventSelect2.append(option).trigger('change');
                            }
                            eventSelect2.trigger({
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

                eventSelect2.select2({                    
                    ajax: {
                        url: "/umbraco/backoffice/uvendiaplugin/EventApi/GetEventsByQuery/",
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
                    placeholder: 'Search for event(s) to select',
                    minimumInputLength: 3,
                    escapeMarkup: function (markup) { return markup; }, // let our custom formatter work
                    templateResult: formatEvent,
                    templateSelection: formatEventSelection
                }).on("change", function (e)
                    {
                        save(e);
                    });

                function formatEvent(event) {
                    if (event.loading) {
                        return event.name;
                    }

                    var markup = 
                        "<div class='select2-result-product__title'>" + event.name + "</div>";                    
                    if (event.startDate) {
                        markup += "<div class='select2-result-product__description'>(" + event.startDate + " - " + event.endDate + ")</div>";
                    }                    

                    return markup;
                }

                function formatEventSelection(event) {
                    if (event.name) {
                        var txt = event.name;
                        if (event.startDate)
                            txt += " (" + event.startDate + " - " + event.endDate + ")";
                        return txt;
                    }
                    else
                        return event.text;
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