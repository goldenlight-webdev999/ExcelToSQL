

jQuery(document).ready(function () {

    // dropdown in leftmenu
    jQuery('.leftmenu .dropdown > a').live("click", function () {
        $('.leftmenu .dropdown').removeClass('active');
        jQuery('.leftmenu .dropdown a').next().slideUp('fast');
        if (!jQuery(this).next().is(':visible')) {
            $(this.parentElement).addClass('active');
            jQuery(this).next().slideDown('fast');
        }
        else
            jQuery(this).next().slideUp('fast');
        return false;
    });

    jQuery('.leftmenu .dropdown > a').mouseover("contextmenu", function (e) {
        e.preventDefault();
        jQuery("#cntnr").css("left", e.pageX + 20);
        jQuery("#cntnr").css("top", e.pageY);
        jQuery("#cntnr").fadeIn(100, startFocusOut());
    });

    //if (jQuery.uniform)
    //    jQuery('input:checkbox, input:radio, .uniform-file').uniform();

    if (jQuery('.widgettitle .close').length > 0) {
        jQuery('.widgettitle .close').click(function () {
            jQuery(this).parents('.widgetbox').fadeOut(function () {
                jQuery(this).remove();
            });
        });
    }


    jQuery(".clickableRow").live("click", function () {
        jQuery('.clickableRow').removeClass('success');
        jQuery(this).addClass('success');
    });


    // add menu bar for phones and tablet
    jQuery('<div class="topbar"><a class="barmenu">' +
		    '</a></div>').insertBefore('.mainwrapper');

    jQuery('.topbar .barmenu').click(function () {

        var lwidth = '170px';
        if (jQuery(window).width() < 340) {
            lwidth = '150px';
        }

        if (!jQuery(this).hasClass('open')) {
            jQuery('.rightpanel, .headerinner, .topbar').css({ marginLeft: lwidth }, 'fast');
            jQuery('.logo, .leftpanel').css({ marginLeft: 0 }, 'fast');
            jQuery(this).addClass('open');
        } else {
            jQuery('.rightpanel, .headerinner, .topbar').css({ marginLeft: 0 }, 'fast');
            jQuery('.logo, .leftpanel').css({ marginLeft: '-' + lwidth }, 'fast');
            jQuery(this).removeClass('open');
        }
    });

    // show/hide left menu
    jQuery(window).resize(function () {
        if (!jQuery('.topbar').is(':visible')) {
            jQuery('.rightpanel, .headerinner').css({ marginLeft: '170px' });
            jQuery('.logo, .leftpanel').css({ marginLeft: 0 });
        } else {
            jQuery('.rightpanel, .headerinner').css({ marginLeft: 0 });
            jQuery('.logo, .leftpanel').css({ marginLeft: '-170px' });
        }
    });

    jQuery('#mainmenu, #administrator').click(function () {
        //alert(jQuery(this).attr('id'));
        var lwidth = '170px';
        if (jQuery(window).width() < 340) {
            lwidth = '150px';
        }

        if (!jQuery(this).hasClass('open')) {
            jQuery('.rightpanel, .headerinner, .topbar').css({ marginLeft: lwidth }, 'fast');
            jQuery('.logo, .leftpanel').css({ marginLeft: 0 }, 'fast');
            jQuery(this).addClass('open');
        } else {
            jQuery('.rightpanel, .headerinner, .topbar').css({ marginLeft: 0 }, 'fast');
            jQuery('.logo, .leftpanel').css({ marginLeft: '-' + lwidth }, 'fast');
            jQuery(this).removeClass('open');
        }
    });

    //Mouse over
    //jQuery('.rightpanel').mouseover(function () {
    //    var lwidth = '170px';
    //    if (jQuery(window).width() < 340) {
    //        lwidth = '150px';
    //    }

    //    if (!jQuery(this).hasClass('open')) {
    //        jQuery('.rightpanel, .headerinner, .topbar').css({ marginLeft: lwidth }, 'fast');
    //        jQuery('.logo, .leftpanel').css({ marginLeft: 0 }, 'fast');
    //        jQuery(this).addClass('open');
    //    } else {
    //        jQuery('.rightpanel, .headerinner, .topbar').css({ marginLeft: 0 }, 'fast');
    //        jQuery('.logo, .leftpanel').css({ marginLeft: '-' + lwidth }, 'fast');
    //        jQuery(this).removeClass('open');
    //    }
    //});

    // dropdown menu for profile image
    jQuery('.userloggedinfo img').click(function () {
        if (jQuery(window).width() < 480) {
            var dm = jQuery('.userloggedinfo .userinfo');
            if (dm.is(':visible')) {
                dm.hide();
            } else {
                dm.show();
            }
        }
    });

    // change skin color
    jQuery('.skin-color a').click(function () { return false; });
    jQuery('.skin-color a').hover(function () {
        var s = jQuery(this).attr('href');
        if (jQuery('#skinstyle').length > 0) {
            if (s != 'default') {
                jQuery('#skinstyle').attr('href', 'css/style.' + s + '.css');
                jQuery.cookie('skin-color', s, { path: '/' });
            } else {
                jQuery('#skinstyle').remove();
                jQuery.cookie("skin-color", '', { path: '/' });
            }
        } else {
            if (s != 'default') {
                jQuery('head').append('<link id="skinstyle" rel="stylesheet" href="css/style.' + s + '.css" type="text/css" />');
                jQuery.cookie("skin-color", s, { path: '/' });
            }
        }
        return false;
    });

    // load selected skin color from cookie
    if (jQuery.cookie('skin-color')) {
        var c = jQuery.cookie('skin-color');
        if (c) {
            jQuery('head').append('<link id="skinstyle" rel="stylesheet" href="css/style.' + c + '.css" type="text/css" />');
            jQuery.cookie("skin-color", c, { path: '/' });
        }
    }


    // expand/collapse boxes
    if (jQuery('.minimize').length > 0) {

        jQuery('.minimize').click(function () {
            if (!jQuery(this).hasClass('collapsed')) {
                jQuery(this).addClass('collapsed');
                jQuery(this).html("&#43;");
                jQuery(this).parents('.widgetbox')
										      .css({ marginBottom: '20px' })
												.find('.widgetcontent')
												.hide();
            } else {
                jQuery(this).removeClass('collapsed');
                jQuery(this).html("&#8211;");
                jQuery(this).parents('.widgetbox')
										      .css({ marginBottom: '0' })
												.find('.widgetcontent')
												.show();
            }
            return false;
        });

    }

});