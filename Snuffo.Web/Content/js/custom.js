; (function ($) {

})(jQuery);



$(function () {
    initPasswordMeter();

    toggleShippingAddress($("#same_address"));

    $("#same_address").click(function () {
        toggleShippingAddress($(this));
    });

    var alertMessage = $("#hdnWebAlertMessage").val();
    if (alertMessage !== null && alertMessage !== "") {
        var alertMessageType = $("#hdnWebAlertMessageType").val();
        var alertTitle = $("#hdnWebAlertTitle").val();
        showAlertMessage(alertTitle, alertMessage, alertMessageType);
    }

    $('#ddlSize').change(function () {
        var pid = $('#hdnProductId').val();
        $.ajax({
            cache: false,
            url: '/umbraco/surface/service/getproductprice/?p=' + pid + '&size=' + $(this).val(),
            dataType: 'html',
            type: 'GET',
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $('#container-price').html(errorThrown);
            },
            success: function (data, textStatus, XMLHttpRequest) {
                $('#container-price').html(data);
            },
            complete: function (XMLHttpRequest, textStatus) {
            }
        });
    });


    // Check out-of-stock
    var addToCartButtons = $('.btnAddToCart');
    if (addToCartButtons.length) {
        var arrProductIds = [];
        addToCartButtons.each(function () {
            arrProductIds.push(parseInt($(this).data('product-id')));
        });
        
        $.ajax({
            cache: false,
            url: '/umbraco/surface/service/getproductstockindication',
            data: { productIds: arrProductIds },
            dataType: 'json',
            type: 'POST',
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                
            },
            success: function (data, textStatus, XMLHttpRequest) {
                
                for (var i = 0; i < data.length; i++) {
                    if (data[i].IsOutOfStock) {
                        var button = addToCartButtons.filter('[data-product-id=' + data[i].ProductId + ']');
                        button.attr('disabled', 'disabled');
                        button.html('Out of Stock');
                        button.toggleClass("btn-outline-primary btn-outline-danger");
                        button.toggleClass("btn-primary btn-danger");                        
                    }                    
                }
            },
            complete: function (XMLHttpRequest, textStatus) {
            }
        });
    }

    $('.btnAddToCart').click(function () {
        var pId = $(this).data('product-id');
        var hdnProductId = $('#hdnProductId');
        var ddlSize = $('#ddlSize');
        var size = '';

        if (hdnProductId.length) {
            pId = hdnProductId.val();
        }
        if (ddlSize.length) {
            size = ddlSize.val();
        }
        var culture = $('#hdnCurrentCulture').val();
        
        var quantity = 1;
        if ($('#quantity').length) {
            quantity = parseInt($('#quantity').val());
        }
        var thumb = $(this).data('thumb');

        var item = { p: pId, qnty: quantity, thumb: thumb, size: size, culture: culture };
        $.ajax({
            cache: false,
            url: '/umbraco/surface/service/addtocart',
            data: { item: item },
            dataType: 'html',
            type: 'POST',
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                showAlertMessage('Error adding product to cart', errorThrown, 'error');
            },
            success: function (data, textStatus, XMLHttpRequest) {
                showAlertMessage('Added to cart', 'Product successfuly added to cart!', 'success');
                $('#cartShop').html(data);
            },
            complete: function (XMLHttpRequest, textStatus) {
            }
        });

    });

    $('#clearCart').click(function () {
        var cartPage = $('#cartPage');

        $.ajax({
            cache: false,
            url: '/umbraco/surface/service/clearcart',
            dataType: 'html',
            type: 'POST',
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                showAlertMessage('Error clearing cart', errorThrown, 'error');
            },
            success: function (data, textStatus, XMLHttpRequest) {
                $('#cartShop').html(data);
                cartPage.html('');
            },
            complete: function (XMLHttpRequest, textStatus) {
            }
        });
    });

    $('.remove-from-cart').click(function () {
        var cartItemId = $(this).data('cart-item-id');
        var row = $(this).closest('tr');
        var table = row.closest('table');
        var footer = $('.shopping-cart-footer');
        var subtotal = $('span#subtotal');

        $.ajax({
            cache: false,
            url: '/umbraco/surface/service/removeFromCart?ci=' + cartItemId,
            dataType: 'html',
            type: 'POST',
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                showAlertMessage('Error removing cart item', errorThrown, 'error');
            },
            success: function (data, textStatus, XMLHttpRequest) {
                $('#cartShop').html(data);
                
            },
            complete: function (XMLHttpRequest, textStatus) {
                row.remove();
                subtotal.css("text-decoration", "line-through");
                if (!table.find('tbody > tr').length) {
                    table.remove();
                    footer.remove();
                }
            }
        });
    });

    $('.openOrderDetails').on('click', function () {
        var orderId = $(this).data('order-id');
        var orderDetailsModal = $('#orderDetails');
        $('.modal-title').text($(this).data('order-title'));

        $.ajax({
            cache: false,
            url: '/umbraco/surface/service/getorderdetails?o=' + orderId,
            dataType: 'html',
            type: 'GET',
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                showAlertMessage('Error removing cart item', errorThrown, 'error');
            },
            success: function (data, textStatus, XMLHttpRequest) {
                $('.modal-body').html(data);
                
            },
            complete: function (XMLHttpRequest, textStatus) {
                orderDetailsModal.modal({ show: true });
            }
        });
    }); 



});

function toggleShippingAddress(sender) {
    if (!sender.is(":checked")) {
        $("#container-shippingAddress").show();
    } else {
        $("#container-shippingAddress").hide();
    }
}

function enableReviewSubmitBtn() {
    document.getElementById("btnReview").disabled = false;
}

function enableSubmitBtn() {
    document.getElementById("btnSubmit").disabled = false;
}

function showAlertMessage(title, message, type) {
    $.toast({
        text: message,
        icon: type,
        //loaderBg: '#9EC600',  // To change the background
        position: 'top-center',
        heading: title,
        hideAfter: 3000,
        showHideTransition: 'fade',
    });
}

function initPasswordMeter() {
    var meter = document.getElementById('password-strength-meter');

    if (meter !== null) {
        var password = document.getElementById('reg-pass');
        var text = document.getElementById('password-strength-text');

        var strength = {
            0: "Worst ☹",
            1: "Bad ☹",
            2: "Weak ☹",
            3: "Good ☺",
            4: "Strong ☻"
        };


        password.addEventListener('input', function () {
            var val = password.value;
            var result = zxcvbn(val);

            // Update the password strength meter
            meter.value = result.score;

            // Update the text indicator
            if (val !== "") {
                $('#password-strength-text').show();
                text.innerHTML = "Strength: " + "<strong>" + strength[result.score] + "</strong>" + "<span class='feedback'>" + result.feedback.warning + " " + result.feedback.suggestions + "</span";
            }
            else {
                $('#password-strength-text').hide();
                text.innerHTML = "";
            }
        });
    }
}