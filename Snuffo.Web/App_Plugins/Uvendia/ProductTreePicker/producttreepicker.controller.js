var app = angular.module("umbraco");
app.controller("Uvendia.ProductTreePickerController",
    function ($scope, productTreePickerResource, assetsService) {
               
        assetsService
            .load([
                "~/Umbraco/Uvendia/Content/jstree/jstree.min.js"
            ])
            .then(function () {                

                $("#jstree").jstree({
                    "checkbox": {
                        "keep_selected_style": false,
                        "three_state": false,
                        "tie_selection": true
                    },
                    "plugins": ["checkbox"],

                    'core': {
                        'data': {
                            'url': function (node) {
                                return node.id === '#' ?
                                    '/umbraco/backoffice/uvendiaplugin/CatalogApi/GetJsTreeStoreRootJson/' :
                                    '/umbraco/backoffice/uvendiaplugin/CatalogApi/GetJsTreeSubNodeJson/';
                            },
                            'data': function (node) {
                                return { 'id': node.id, 'selected': $scope.model.value };
                            }
                        }
                    }
                }).on("select_node.jstree deselect_node.jstree", function (e, data) {                     
                    var selected = null;
                    var ids = [];
                    for (i = 0; i < data.selected.length; i++) {
                        var n = data.selected[i];
                        n = n.substring(n.lastIndexOf('_') + 1);  
                        if ($.inArray(n, ids) === -1) {
                            ids.push(n);
                            if (selected === null)
                                selected = n;
                            else
                                selected += ',' + n;
                        }
                    }                    
                    
                    $scope.model.value = selected;    
                    //console.log('model.value', $scope.model.value)
                });
            });        
        
        //load the separate css for the editor to avoid it blocking our js loading
        assetsService.loadCss("~/Umbraco/Uvendia/Content/jstree/themes/default/style.min.css");
       
    });