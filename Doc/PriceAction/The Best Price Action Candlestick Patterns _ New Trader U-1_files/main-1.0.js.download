jQuery(document).ready (function ($) {
"use strict";
	// remove empty p
	$('p')
	.filter(function() {
	    return $.trim($(this).text()) === '' && $(this).children().length == 0
	})
	.remove();

//place holder
    $('input').each(function() {
    $(this).data('holder',$(this).attr('placeholder'));

    $('input').focusin(function(){
        $(this).attr('placeholder','');
    });
    $('input').focusout(function(){
        $(this).attr('placeholder',$(this).data('holder'));
    });
        });
    $('textarea').each(function() {
	    $(this).data('holder',$(this).attr('placeholder'));

	    $(this).focusin(function(){
	        $(this).attr('placeholder','');
	    });
	    $(this).focusout(function(){
	        $(this).attr('placeholder',$(this).data('holder'));
	    });
    });
//alert-bar
if (jQuery(".alert-bar").length) {
        $('.alert-bar').slideDown('600');

    setTimeout(function() {
        $('.alert-bar').slideUp('800', function() {
        	$('.alert-bar').remove();
        })
    }, 5000);
}
// search
$('#navigation .nav-button').click(function(e) {
    if (!$(this).hasClass('active')) {
	$('#navigation .nav-button').removeClass('active');
	$(this).addClass('active');
	$('.nb-inner-wrap').removeClass('sw-show');
	$(this).next('.nb-inner-wrap').addClass('sw-show');
    } else {
	$(this).removeClass('active');
	$('.nb-inner-wrap').removeClass('sw-show');
    }
    e.stopPropagation();

});
$('.nb-inner-wrap').click(function(e) {
    e.stopPropagation();
});

$('body').click(function(e) {
    $('#navigation .nav-button').removeClass('active');
    $('.nb-inner-wrap').removeClass('sw-show');
});

//Sticky navigation
if ($(window).width() > 1000) {
   if ($('body').hasClass('sticky_navigation_on')) {
        var aboveHeight = $('#header-wrapper').outerHeight();
        $(window).scroll(function(){
	        //if scrolled down more than the headerÕs height
                if ($(window).scrollTop() > aboveHeight){
	        // if yes, add ÒfixedÓ class to the <nav>
	        // add padding top to the #content
            if ( $('#wpadminbar').length ) {
                $('#navigation').addClass('sticky-nav').css('top','28px').next().css('padding-top','52px');
             } else {
                $('#navigation').addClass('sticky-nav').css('top','0').next().css('padding-top','52px');
            }
                } else {

	        // when scroll up or less than aboveHeight,
                $('#navigation').removeClass('sticky-nav').css('top', 0).next().css('padding-top','0');
                }
        });
    }
}
//tabbed widget
    jQuery(".widget_momizattabber").each(function(){
        var ul = jQuery(this).find(".main_tabs ul.tabs");

        jQuery(this).find(".tab-content").each(function() {
            jQuery(this).find('a.mom-tw-title').wrap('<li></li>').parent().detach().appendTo(ul);
        });
    });

// secondary sidebar in all devices

if (! $('body').hasClass('responsive_disabled') && $('body').hasClass('show_secondary_sidebar_on_ipad')) {
if ($(window).width() < 1310) {
	$('.secondary-sidebar').show();
	$('.secondary-sidebar').insertBefore('.main-sidebar');
	$('.secondary-sidebar').removeClass('secondary-sidebar vc_sec_sidebar alignlefti alignrighti').addClass('main-sidebar vc_sidebar moded');
	if($(window).width() > 1000) {
		$('.vc_column_container.main-sidebar.moded').css('margin-right', '15px');
	}
}

$(window).resize(function() {
if ($(window).width() < 1310) {
	$('.secondary-sidebar').show();
	$('.secondary-sidebar').insertBefore('.main-sidebar');
	$('.secondary-sidebar').removeClass('secondary-sidebar vc_sec_sidebar alignlefti alignrighti').addClass('main-sidebar vc_sidebar moded');
	if($(window).width() > 1000) {
		$('.vc_column_container.main-sidebar.moded').css('margin-right', '15px');
	}
}
});
}

// Main Tabs
    if ($(".main_tabs ul.tabs").length) { $("ul.tabs").momtabs("div.tabs-content-wrap > .tab-content", { effect: 'fade'}); }

//HIDPI Images
    var hidpi = window.devicePixelRatio > 1 ? true : false;
    if (hidpi) {
    // Replace img src with data-hidpi
    $('img[data-hidpi]').each(function() {
    // If width x height hasn't been set, fill it in
    if ($(this).parents('.tab-content').length === 0) {
	/*
	if ($(this).attr('width') === undefined) {
	$(this).attr('width', $(this).width());
	}
	if ($(this).attr('height') === undefined) {
	$(this).attr('height', $(this).height());
	}
	*/
    }
    $(this).attr('src', $(this).data('hidpi'));
    });
    }
// weather widget
    $('.weather-widget').on('click', '.day-summary',function() {
        var $this = $(this);
        if ($this.hasClass('active')) {
            $(this).next('.day-details').slideToggle(250, function() {
                $this.toggleClass('active');
            });
        } else {
                $this.toggleClass('active');
            $(this).next('.day-details').slideToggle(250);
        }
    });

//time line
    if ($('.mom-timeline').length) {
	$('.tl-month:first-child').removeClass('closed').addClass('opened');
        $('.tl-month').each(function() {
            if ($(this).hasClass('opened')) {
                $(this).find('.tl-days').show();
                $(this).find('.tlm-title .handle').removeClass('brankic-icon-add');
                $(this).find('.tlm-title .handle').addClass('brankic-icon-minus3');
            }
    });
        $('.tl-month .tlm-title .handle').click(function() {
            var $this = $(this);
            var month = $(this).parent().parent();
            if (month.hasClass('closed')) {
                month.removeClass('closed');
                month.addClass('opened');
                month.find('.tl-days').slideDown();
                $this.removeClass('brankic-icon-add');
                $this.addClass('brankic-icon-minus3');
            } else {
                month.removeClass('opened');
                month.find('.tl-days').slideUp('normal', function() {
                month.addClass('closed');
                $this.removeClass('brankic-icon-minus3');
                $this.addClass('brankic-icon-add');
                });
            }
        });
    }

//social icons
if ($('.mom-social-icons').length) {
    $('.mom-social-icons li').each(function () {
    var dataHover = $('a',this).attr('data-bghover');
    if (typeof dataHover !== 'undefined' && dataHover !== false) {
	 var origBg = $('a',this).css('background');
	 var hoverBg = $('a',this).data('bghover');
	$('a', this).hover(function() {
	    $(this).css('background', hoverBg)
	}, function() {
	    $(this).css('background', origBg)
	});
    }
    });
}

//images
if(!( /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) )) {
    // fade images on appear
    $('body.fade-imgs-in-appear .main_container img, body.fade-imgs-in-appear .sidebar img, body.fade-imgs-in-appear #footer img, body.fade-imgs-in-appear .wpb_row img').addClass('disappear');
    $('body.fade-imgs-in-appear .main_container img, body.fade-imgs-in-appear .sidebar img, body.fade-imgs-in-appear #footer img,  body.fade-imgs-in-appear .wpb_row img').one('inview', function() {$(this).addClass('mom_appear');});

    //body background link
    $('body.use_bg_as_ad.layout-boxed').click(function(e){
	if (e.target === this) {

			window.open(
			  momAjaxL.bodyad,
			  '_blank' // <- This is what makes it open in a new window.
			);

	}
    });
}

// responsive videos
$('.video_frame').each(function(index, el) {
	var t = $(this);
	var w = t.width();
	var h = w/16;
	h = h*9;

	t.find('iframe').css('height', h+'px');

});
window.onresize = function(event) {
$('.video_frame').each(function(index, el) {
	var t = $(this);
	var w = t.width();
	var h = w/16;
	h = h*9;

	t.find('iframe').css('height', h+'px');

});
};

$('.entry-content .video_frame iframe').each(function(index, el) {
	var t = $(this);
	var w = t.width();
	var h = w/16;
	h = h*9;

	t.css('height', h+'px');

});
window.onresize = function(event) {
$('.entry-content .video_frame iframe').each(function(index, el) {
	var t = $(this);
	var w = t.width();
	var h = w/16;
	h = h*9;

	t.css('height', h+'px');

});
};
//Submenu auto align
        $('ul.main-menu > li').each(function(e){
            var t = $(this),
                submenu = t.find('.cats-mega-wrap');
            if( submenu.length > 0 ) {
                var offset = submenu.offset(),
                    w = submenu.width();
                if( offset.left + w > $(window).width() ) {
                    t.addClass('sub-menu-left');
                } else {
                    t.removeClass('sub-menu-left');
                }
            }
        });
//category mega menu
	$('.cats-mega-wrap ul.sub-menu li').mouseenter(function() {
	    var id = $(this).attr('id');
	    var id = id.split('-');
	    //console.log(id[2]);
	    $(this).parent().find('li').removeClass('active');
	    $(this).addClass('active');
	    $(this).parent().next('.subcat').find('.mom-cat-latest').hide();
	    $(this).parent().next('.subcat').find('#mn-latest-'+id[2]).show();
	});

//breaking news
if ($('.news-ticker').length) {
	var timeout = $('.news-ticker').data('timeout');
    $('body:not(.rtl) .news-ticker:not(.custom-animation) > ul').liScroll();
    $('body.rtl .news-ticker:not(.custom-animation) > ul').liScrollRight();
    $('.news-ticker.animation-updown ul').newsTicker({
    row_height: 39,
    max_rows: 1,
    speed: 600,
    direction: 'up',
    duration: timeout,
    autostart: 1,
    pauseOnHover: 1
});
/*
$('.news-ticker ul').each(function() {
	var t = $(this),
			w = 0;

	t.children('li').each(function() {
		w += $(this).width() + 20;
	});
	t.width(w)
});
*/

if ($('body').hasClass('ticker_has_live_time')) {
$.startTime = function () {
    var today=new Date();
    var h=today.getHours();
    var m=today.getMinutes();
    var s=today.getSeconds();
    m = $.checkTime(m);
    s = $.checkTime(s);
    if ($('body').hasClass('time_in_twelve_format')) {
	h = ((h+11) % 12) +1;
    }
    //document.getElementById('txt').innerHTML = h+":"+m+":"+s;
    $('span.current_time span').html(h+":"+m+":"+s);
    var t = setTimeout(function(){$.startTime()},500);
}
$.checkTime = function(i) {
    if (i<10) {i = "0" + i};  // add zero in front of numbers < 10
    return i;
}
$.startTime();
}

}

// smooth scroll
if ($('body').hasClass('smooth_scroll_enable')) {
	$("html").niceScroll();
}
//social share
$('.mom-social-share a').click(function(e) { e.preventDefault(); });

//twitter widget buttons
$('.twiter-list ul.twiter-buttons li a').click( function(e) {
    e.preventDefault();
});

//Porfolio filter
$('ul.portfolio-filter').each( function() {
	var $this = $(this);
	$this.find('li a').click(function() {
	    $this.find('li').removeClass('current');
	    $(this).parent().addClass('current');
	});
});

// Avanced search form Validate
$('.advanced-search-form [type="submit"]').click(function(e) {
    var s = $(this).parent().find('input[name="s"]');
    if (s.val() === '' ) {
	e.preventDefault()
	s.addClass('invalid');
	s.attr('placeholder', s.data('nokeyword'));
    }
});

// videos
$('.mom-video-widget').fitVids();
//scroll to top
$('.scrollToTop').hide();
	$(window).scroll(function () {
		if( $(this).scrollTop() > 100 ) {
			$('.scrollToTop').fadeIn(300);
		}
		else {
			$('.scrollToTop').fadeOut(300);
		}
	});

	$('.scrollToTop').click(function(){
		$('html, body').animate({scrollTop:0}, 500 );
		return false;
	});
// sidebars categories
$('.sidebar li.cat-item, .sidebar .widget_archive li').each(function(){
          var $contents = $(this).contents();
          if ($contents.length > 1)  {
	  $contents.eq(1).wrap('<span class="cat_num"></span>');

	  $contents.eq(1).each(function(){
	});
    }
    }).contents();
	    $('.sidebar li.cat-item .cat_num, .sidebar .widget_archive li .cat_num').each(function () {
	       $(this).html($(this).text().substring(2));
	      $(this).html( $(this).text().replace(/\)/gi, "") );
	    });

if ($('.sidebar li.cat-item').length) {
    $('.sidebar li.cat-item .cat_num .line').each( function() {
	if ($(this).is(':empty')){
	    $(this).parent().hide();
	}

});
}
// footer Categories
$('#footer li.cat-item').parent().addClass('two_columns_categoreis clearfix');
/*-----------------------------------------
*	Shortcode
*-----------------------------------------*/

//Accordion
$('.accordion.mom_accordion').each( function() {
    var acc = $(this);
    if (acc.hasClass('toggle_acc')) {
        acc.find('li:first .acc_title').addClass('active');
        acc.find('.acc_toggle_open').addClass('active');
        acc.find('.acc_toggle_open').next('.acc_content').show();
        acc.find('.acc_toggle_close').removeClass('active');
        acc.find('.acc_toggle_close').next('.acc_content').hide();
        acc.find('.acc_title').click(function() {
        $(this).toggleClass('active');
        $(this).next('.acc_content').slideToggle();
    });
    } else {
    acc.find('li:first .acc_title').addClass('active');
    acc.find('.acc_title').click(function() {
        if(!$(this).hasClass('active')) {
        acc.find('.acc_title').removeClass('active');
        acc.find('.acc_content').slideUp();
        $(this).addClass('active');
        $(this).next('.acc_content').slideDown();
        }
    });
    }
});
$(".accordion").each(function () {
    $(this).find('.acc_title').each(function(i) {
        $(this).find('.acch_numbers').text(i+1);
    });
});

// lists
if ($('.mom_list').length) {
    $('.mom_list li').each(function() {
	var i = $(this).children('i');
	var cl = i.data('color');
	var clh = i.data('color_hover');
	var bg = i.data('bg');
	var bgh = i.data('bg_hover');
	if (clh !== '') {
	    $(this).hover(function() {
		i.css('color', clh);
	    }, function() {
		i.css('color', cl);
	    });
	}
	if (bgh !== '') {
	    $(this).hover(function() {
		i.css('background', bgh);
	    }, function() {
		i.css('background', bg);
	    });
	}


    });
}
//callitout
if ($('.mom_callout').length) {
    $('.mom_callout').each( function () {
	if ($(this).find('.cobtr').length) {
	var btwidth = parseFloat($(this).find('.cobtr').css('width'))+30;
	var btheight = parseFloat($(this).find('.cobtr').css('height'))/2;
	$(this).find('.callout_content').css('margin-right',btwidth+'px');
	$(this).find('.cobtr').css('margin-top', '-'+btheight+'px');
	}
	if ($(this).find('.cobtl').length) {
	var btwidth = parseFloat($(this).find('.cobtl').css('width'))+30;
	var btheight = parseFloat($(this).find('.cobtl').css('height'))/2;
	$(this).find('.callout_content').css('margin-left',btwidth+'px');
	$(this).find('.cobtl').css('margin-top', '-'+btheight+'px');
	}
    });
}
	jQuery('.mom_button').hover(
		function(){
		var $hoverbg = jQuery(this).attr('data-hoverbg');
		var $texthcolor = jQuery(this).attr('data-texthover');
		var $borderhover = jQuery(this).attr('data-borderhover');
		jQuery(this).css("background-color",$hoverbg);
		jQuery(this).css("color",$texthcolor);
		jQuery(this).css("border-color",$borderhover);
	},function() {
		var $bgcolor = jQuery(this).attr('data-bg');
		var $textColor = jQuery(this).attr('data-text');
		var $bordercolor = jQuery(this).attr('data-border');
		if($bgcolor!==undefined){
			jQuery(this).css("background-color",$bgcolor);
		}else {
			jQuery(this).css("background-color",'');
		}
		if($textColor!==undefined){
			jQuery(this).css("color",$textColor);
		}else {
			jQuery(this).css("color",'');
		}
		if($bordercolor !== undefined){
			jQuery(this).css("border-color",$bordercolor);
		}else {
			jQuery(this).css("border-color",'');
		}
	});
// Tab Current icon
if (('.main_tabs ul.tabs li a i').length) {
    $('.main_tabs').each(function () {
	var $this = $(this);
	var current_tab = $this.find('.tabs li a.current i[class*="-icon-"]');
	current_tab.css('color', current_tab.attr('data-current'));
	$this.find('.tabs li a').click(function () {
	if ($(this).hasClass('current')) {
	var $current = $(this).find('[class*="-icon-"]').attr('data-current');
	var $orig = $(this).find('[class*="-icon-"]').attr('data-color');

	$this.find('.tabs li a i[class*="-icon-"]').css('color',$orig);
	$('[class*="-icon-"]', this).css('color', $current);
	}
	});
    });
}
// Accordion Current icon
if (('h2.acc_title i').length) {
    $('.accordion').each(function () {
	var $this = $(this);
	var current_acc = $this.find('h2.active i[class*="-icon-"]');
	current_acc.css('color', current_acc.attr('data-current'));
	$this.find('h2.acc_title').click(function () {
	if ($(this).hasClass('active')) {
	var $current = $(this).find('[class*="-icon-"]').attr('data-current');
	var $orig = $(this).find('[class*="-icon-"]').attr('data-color');

	$this.find('h2.acc_title i[class*="-icon-"]').css('color',$orig);
	$('[class*="-icon-"]', this).css('color', $current);
	}
	});
    });
}

//team members
	var tm_cols = 2;
	var tm_2_i = 0;
	$(".team_member2").each(function(){
		tm_2_i++;
		tm_cols = 2;
		if (tm_2_i % tm_cols === 0) {$(this).addClass("last");}
	});
	var tm_3_i = 0;
	$(".team_member3").each(function(){
		tm_3_i++;
		tm_cols = 3;
		if (tm_3_i % tm_cols === 0) {$(this).addClass("last");}
	});
	var tm_4_i = 0;
	$(".team_member4").each(function(){
		tm_4_i++;
		tm_cols = 4;
		if (tm_4_i % tm_cols === 0) {$(this).addClass("last");}
	});
	var tm_5_i = 0;
	$(".team_member5").each(function(){
		tm_5_i++;
		tm_cols = 5;
		if (tm_5_i % tm_cols === 0) {$(this).addClass("last");}
	});
$('.team_member').each( function () {
    var socials = $(this).find('.member_social ul li');
    var width = 100/socials.length;
    socials.css('width',width+'%');
});

//Icon Colors in hover
jQuery('.mom_iconbox').hover(
	function(){
	var icon = $(this).find('[class*="-icon-"]');
	var icon_wrap = $(this).find('.iconb_wrap');

	var $hover = icon.attr('data-hover');
	var $bghover = icon_wrap.attr('data-hover');
	var $bdhover = icon_wrap.attr('data-border_hover');

	if ($hover !== '') {
	icon.css("color",$hover);
	}
	if ($bghover !== '') {
	icon_wrap.css("background",$bghover);
	}
	if ($bdhover !== '') {
	icon_wrap.css("border-color",$bdhover);
	}
},function() {
	var icon = $(this).find('[class*="-icon-"]');
	var icon_wrap = $(this).find('.iconb_wrap');

	var $color = icon.attr('data-color');
	var $origcolor = icon.css('color');
	var $bgcolor = icon_wrap.attr('data-color');
	var $origbg = icon_wrap.css('background-color');
	var $bdcolor = icon_wrap.attr('data-border_color');
	var $origbd = icon_wrap.css('border-color');
	if($color!==undefined){
		icon.css("color",$color);
	}else {
		icon.css("color",$origcolor);
	}
	if($bgcolor!==undefined){
		icon_wrap.css("background",$bgcolor);
	}else {
		icon_wrap.css("background",$origbg);
	}
	if($bdcolor!==undefined){
		icon_wrap.css("border-color",$bdcolor);
	}else {
	}
});

//icona
jQuery('.mom_icona').hover(
	function(){
	var icon = $(this).find('[class*="-icon-"]');
	var icon_wrap = $(this);
	var $hover = icon.attr('data-hover');
	var $bghover = icon_wrap.attr('data-hover');
	var $bdhover = icon_wrap.attr('data-border_hover');
	icon.css("color",$hover);
	icon_wrap.css("background",$bghover);
	icon_wrap.css("border-color",$bdhover);
},function() {
	var icon = $(this).find('[class*="-icon-"]');
	var icon_wrap = $(this);
	var $color = icon.attr('data-color');
	var $origcolor = icon.css('color');
	var $bgcolor = icon_wrap.attr('data-color');
	var $origbg = icon_wrap.css('background-color');
	var $bdcolor = icon_wrap.attr('data-border_color');
	var $origbd = icon_wrap.css('border-color');
	if($color!==undefined){
		icon.css("color",$color);
	}else {
		icon.css("color",$origcolor);
	}
	if($bgcolor!==undefined){
		icon_wrap.css("background",$bgcolor);
	}else {
		icon_wrap.css("background",$origbg);
	}
	if($bdcolor!==undefined){
		icon_wrap.css("border-color",$bdcolor);
	}else {
	}
});
//Porfolio filter
$('.protfolio_filter ul').each( function() {
	var $this = $(this);
	$this.find('li a').click(function() {
	$this.find('li').removeClass('current');
	$(this).parent().addClass('current');
	});
});

//heighest col
/*
var highestCol = Math.max($('.bothsides_content').height(),$('.sideb').height());
$('.bothsides_content, .sideb').height(highestCol);
*/

// comment form
if($('#commentform').length) {
$('#commentform input').each(function() {
$(this).data('holder',$(this).attr('placeholder'));

$('#commentform input').focusin(function(){
    $(this).attr('placeholder','');
});
$('#commentform input').focusout(function(){
    $(this).attr('placeholder',$(this).data('holder'));
});
    });
$('#commentform #comment').data('holder',$('#commentform #comment').attr('placeholder'));

$('#commentform #comment').focusin(function(){
    $(this).attr('placeholder','');
});
$('#commentform #comment').focusout(function(){
    $(this).attr('placeholder',$(this).data('holder'));
});
}

// Contact form
if($('.mom_contact_form').length) {
$('.mom_contact_form input').each(function() {
	$(this).data('holder',$(this).attr('placeholder'));

	$('.mom_contact_form input').focusin(function(){
	$(this).attr('placeholder','');
	});
	$('.mom_contact_form input').focusout(function(){
	$(this).attr('placeholder',$(this).data('holder'));
	});
});

$('.mom_contact_form textarea').each(function() {
	$(this).data('holder',$(this).attr('placeholder'));
	$('.mom_contact_form textarea').focusin(function(){
$(this).attr('placeholder','');
	});
	$('.mom_contact_form textarea').focusout(function(){
$(this).attr('placeholder',$(this).data('holder'));
	});
});

}
//share
if ($('.mom_share_buttons').length) {
    $('.mom_share_buttons').data('height',$('.mom_share_buttons').css('height'));
    var curHeight = $('.mom_share_buttons').height();
    $('.mom_share_buttons').css('height', 'auto');
    var autoHeight = $('.mom_share_buttons').height();
    $('.mom_share_buttons').css('height', curHeight);
    $('.mom_share_it .sh_arrow').toggle(function () {
	$('.mom_share_buttons').stop().animate({height: autoHeight}, 300);
	$(this).find('i').removeClass();
	$(this).find('i').addClass('momizat-icon-193');
    }, function () {
	$('.mom_share_buttons').stop().animate({height: $('.mom_share_buttons').data('height')}, 300);
	$(this).find('i').removeClass();
	$(this).find('i').addClass('momizat-icon-194');
    });
}

//toggles
jQuery("h4.toggle_title").click(function () {
	$(this).next(".toggle_content").slideToggle();
	$(this).toggleClass("active_toggle");
	$(this).parent().toggleClass("toggle_active");
});

$("h4.toggle_min").click(function () {
	$(this).next(".toggle_content_min").slideToggle();
	$(this).toggleClass("active_toggle_min");
});

//scroll to top
$('.scrollTo_top').hide();
	$(window).scroll(function () {
		if( $(this).scrollTop() > 100 ) {
			$('.scrollTo_top').fadeIn(300);
		}
		else {
			$('.scrollTo_top').fadeOut(300);
		}
	});

	$('.scrollTo_top').click(function(){
		$('html, body').animate({scrollTop:0}, 500 );
		return false;
	});

//lightbox
if ($('.mom_lightbox').length) {
$(".mom_lightbox > a").prettyPhoto({animation_speed:'fast',slideshow:10000, deeplinking: false});
}

    $('body.open_images_in_lightbox a').each(function() {
        if(/\.(?:jpg|jpeg|gif|png)$/i.test($(this).attr('href'))){
            $(this).prettyPhoto();
        }
    });
// Mobile
if ($('.top_menu_handle').length) {
    $('.top_menu_handle').toggle( function () {
	$(this).next('.mobile_top_nav').show();
	$(this).addClass('tmh_close');
    }, function () {
	$(this).next('.mobile_top_nav').hide();
	$(this).removeClass('tmh_close');
    });
}

if ($('.mobile_main_nav_handle').length) {
    $('.mobile_main_nav_handle').toggle( function () {
	$(this).next('.mom_mobile_main_nav .nav').slideDown();
    }, function () {
	$(this).next('.mom_mobile_main_nav .nav').slideUp();
    });

}

$(window).resize(function() {
  if ($(window).width() < 978) {
	$('.video_wrap').fitVids();
  }
});

  if ($(window).width() < 978) {
	$('.video_wrap').fitVids();
  }
//$('.video_frame').fitVids();
$(window).resize(function() {
  if ($(window).width() < 460) {
    $('.topbar .mom-social-icons li').hide();
    $(".topbar .mom-social-icons li:lt(7)").show();
  }
});

  if ($(window).width() < 460) {
    $('.topbar .mom-social-icons li').hide();
    $(".topbar .mom-social-icons li:lt(7)").show();
  }
//$("html").niceScroll();

/*
     $('a').click(function(){
	$('html, body').animate({
	    scrollTop: $('[name="' + $.attr(this, 'href').substr(1) + '"]').offset().top
	}, 500);
	return false;
    });
 */
$('.animator.animated, .iconb_wrap.animated').each( function() {
    var $this = $(this);
    var animation = $(this).attr('data-animate');

$this.bind('inview', function(event, isInView, visiblePartX, visiblePartY) {
  if (isInView) {
	    $(this).addClass(animation);
	    $(this).css('visibility', 'visible');
	    if(animation.indexOf('fade') === -1)
	    {
	      $(this).css('opacity', '1');
	    }
    if (visiblePartY == 'top') {
      // top part of element is visible
    } else if (visiblePartY == 'bottom') {
      // bottom part of element is visible
    } else {
      // whole part of element is visible
    }
  } else {
    // element has gone out of viewport
  }
});

});
if ($('.progress_outer').length) {
    $('.progress_outer').each( function() {
	var $this = $(this);
    $this.bind('inview', function(event, isInView, visiblePartX, visiblePartY) {
      if (isInView) {
		$(this).find('.parograss_inner').show();
		$(this).find('.parograss_inner').addClass('ani-bar');
	if (visiblePartY == 'top') {
	  // top part of element is visible
	} else if (visiblePartY == 'bottom') {
	  // bottom part of element is visible
	} else {
	  // whole part of element is visible
	}
      } else {
	// element has gone out of viewport
      }
    });

    });
}
//responsive headings
    if ($(window).width() < 767) {
	$('h1,h2,h3,h4,h5,h6, span, .mom_google_font').each(function() {
	    if ($(this).attr('font-size')) {
		var fs = parseFloat($(this).css('font-size'));
		if (fs > 24) {
		    $(this).css('font-size','23px');
		}
	    }
	});
    }

if ($('.mom_custom_background').length) {
    $('.mom_custom_background').each(function() {
	var $this = $(this);
	$(window).scroll(function () {
		var speed = 8.0;
		$this.css({backgroundPosition:(-window.pageXOffset / speed) + "px " + (-window.pageYOffset / speed) + "px"});
		//document.body.style.backgroundPosition = (-window.pageXOffset / speed) + "px " + (-window.pageYOffset / speed) + "px";
	});
    });
}
//Default Gallery
if (! $('body').hasClass('disable_lightbox_in_wp_gallery')) {
	if ($('.gallery .gallery-item').length) {
	    $(".gallery .gallery-item a").attr('rel', 'prettyPhoto[pp_gal]');
	    $(".gallery .gallery-item a").prettyPhoto();
	}
}

//lightbox
if ($('img.lightbox').length) {
    $('img.lightbox').each(function() {
	$(this).parent('a').prettyPhoto();
    })
}

/*----------------------------
    Ads
 ----------------------------*/
if ($('.mca-fixed').length) {
    var mca_top = $('.mca-fixed').offset().top;
    var mca = $('.mca-fixed');
        $(window).scroll(function(){
	    if ($(window).scrollTop() > mca_top){
		if ( $('#wpadminbar').length ) {
		    mca.css({ top:'28px', position: 'fixed' });
		    mca.addClass('mca_touch_top');
		 } else {
		    mca.css({ top:'0', position: 'fixed' });
		    mca.addClass('mca_touch_top');
		}
	    } else {
		mca.css({ top:'auto', position: 'absolute' });
		mca.removeClass('mca_touch_top');
	    }
        });
}


/* ==========================================================================
 *                Responsive mode
   ========================================================================== */

// double tab on navigation
if(( /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) )) {
    $('#navigation .main-menu > li.menu-item-has-children').doubleTapToGo();
}

// Responsive menus
$('.top-menu-holder').click(function(e) {
    e.stopPropagation();
    $('.device-top-nav').slideToggle();
    $(this).toggleClass('active');
});
$('.device-top-nav, .device-menu').click(function(e) {
    e.stopPropagation();
});
$('body').click(function() {
    $('.device-top-nav').slideUp();
    $('.device-menu').slideUp();
});

$('.device-menu-holder').click(function(e) {
    e.stopPropagation();
    if ($(this).hasClass('active')) {
	    $('.device-menu li').each(function() {
		if ($(this).find('.mom_mega_wrap').length !== 0) {
		} else {
		    $(this).find('.sub-menu').slideUp();
		}
	    });
	    $('.device-menu').find('.dm-active').removeClass('dm-active');
	    $('.device-menu').find('.mom_custom_mega').slideUp();
    }
    $('.device-menu').slideToggle();
    $(this).toggleClass('active');
    $('#navigation .nav-buttons').toggleClass('mh-active');

});
$('.responsive-caret').click(function() {
    var li = $(this).closest('li');
    if (li.hasClass('dm-active') || li.find('.dm-active').length !== 0 || li.find('.sub-menu').is(':visible') || li.find('.mom_custom_mega').is(':visible') ) {
	li.removeClass('dm-active');
	li.children('.sub-menu').slideUp();
	if (li.find('.mom_mega_wrap').length === 0) {
	    	li.find('.sub-menu').slideUp();
	}
	if (li.hasClass('mom_default_menu_item') || li.find('.cats-mega-wrap').length !== 0) {
	    li.find('.sub-menu').slideUp();
	}
	li.find('.dm-active').removeClass('dm-active');
	if (li.find('.mom_custom_mega').length !== 0) {
	    li.find('.mom_custom_mega').slideUp();
	}

    } else {
	$('.device-menu').find('.dm-active').removeClass('dm-active');
	li.addClass('dm-active');
	li.children('.sub-menu').slideDown();
	if (li.find('.cats-mega-wrap').length !== 0) {
	    li.find('.sub-menu').slideDown();
	}
	if (li.find('.mom_custom_mega').length !== 0) {
	    li.find('.mom_custom_mega').slideDown();
	}

    }
})
$('.the_menu_holder_area').html($('.device-menu').find('.current-menu-item').children('a').html());

var nbts = $('.nav-buttons .nav-button');
var rnp = 0;
nbts.each(function () {
    var w = $(this).outerWidth()-1;
    rnp += w;
});
    if (nbts.length === 3) {
	rnp = rnp+2;
    }

$('body:not(.rtl) .device-menu-wrap').css('padding-right',rnp+'px');
$('body.rtl .device-menu-wrap').css('padding-left',rnp+'px');

// scrolling box
$(window).resize(function() {
  if ($(window).width() < 670) {
	$('.scrolling-box .sb-item .sb-item-img').each( function(e) {
	    var img = $(this).find('img');
	    img.attr('src', img.data('hidpi'));
	});
  }
    if ($(window).width() < 480) {
	$('.recent-news .news-image, .news-list .news-image, .blog-post .bp-details .post-img').each( function(e) {
	    var img = $(this).find('img');
	    var attr = img.attr('data-hidpi');
	    if (typeof attr !== 'undefined' && attr !== false) {
		img.attr('src', img.data('hidpi'));
	    }
	});
  }
});
  if ($(window).width() < 670) {
	$('.scrolling-box .sb-item .sb-item-img').each( function(e) {
	    var img = $(this).find('img');
	    img.attr('src', img.data('hidpi'));
	});
  }
  if ($(window).width() < 480) {
	$('.recent-news .news-image, .news-list .news-image, .blog-post .bp-details .post-img').each( function(e) {
	    var img = $(this).find('img');
	    var attr = img.attr('data-hidpi');
	    if (typeof attr !== 'undefined' && attr !== false) {
		img.attr('src', img.data('hidpi'));
	    }
	});
  }
  //buddypress
  $('.widget:not(.bos_searchbox_widget_class) select, select#notifications-sort-order-list, select#members-friends, select#groups-sort-by, #members-order-by, #message-type-select, #activity-filter-by').wrap('<div class="mom-select"></div>');
    $('#buddypress div#object-nav.item-list-tabs ul li.last.filter').prev().addClass('mom_last_child');

    //top banner
    $('.top_banner').show();
    $('a.tob_banner_close').on('click', function(e) {
	    $('.top_banner').slideUp(400);
	    var exp = $(this).date('exp');
	    if (exp === '') {
		exp = 7;
	    }
	    if (typeof($.cookie) == "function") {
		$.cookie('tb_clase', 'yes', { expires: exp, path: '/' });
	    }
	e.preventDefault();
    });

    if (typeof($.cookie) == "function") {
	/* if (!$('a.tob_banner_close').hasClass('tb_save_close')) {
	    $('body').css('background', 'green');
	    $.cookie("tb_clase", null);
	    $.removeCookie('tb_clase');
	}*/
	if ($.cookie('tb_clase') === 'yes') {
	    $('.top_banner').hide();
	}
    }


//alert($.cookie('tb_clase'));

	 $('.mom_mega_cats .cats-mega-inner > ul > li').on('mouseenter', function(e) {
	    e.preventDefault();
	    var t = $(this);
	    var tid = t.attr('id');
	    tid = tid.split('-');
	    tid = tid[2];
	    var d = t.parent().next('.subcat').find('#mn-latest-'+tid);
	    var dest = t.parent().next('.subcat').find('#mn-latest-'+tid+' > ul');
	    var id = d.data('id');
	    var object = d.data('object');
	    var layout = d.data('layout');
	    if (dest.children().length === 0) {
			jQuery.ajax({
			    type: "post",
			    url: momAjaxL.url,
			    dataType: 'html',
			    data: "action=mmcl&nonce="+momAjaxL.nonce+"&id="+id+"&object="+object+"&layout="+layout,
			    beforeSend: function() {
					dest.addClass('loading');
			    },
			    success: function(data){
				dest.removeClass('loading');
				dest.html(data);
			    }
			});
    	    }
	});

/*------------------------------------------------
			login widget
-------------------------------------------------*/
$('.mom-login-widget .button.user-submit').click(function () {
	var p = $(this).parents('.mom-login-form');
	var user = p.find('input[name="log"]');
	var pwd = p.find('input[name="pwd"]');
	if (user.val() === '' || pwd.val() === '') {
		user.addClass('error');
		pwd.addClass('error');
		return false;
	}
});
/*------------------------------------------------
			Feature Slider
-------------------------------------------------*/
if ($('.feature-slider').length) {
	$('.feature-slider').each(function() {
		var t = $(this);
		if (!t.hasClass('gallery-post-slider')) {
		var animation = t.data('sanimation');
		var speed = t.data('speed');
		var timeout = t.data('timeout');
		var easing = t.data('easing');
		var rndn = t.data('rndn');
		var rtl = false;
		var autoplay = t.data('autoplay');
		var items = t.data('items');

		var animation_new = t.data('animation_new');
		var animation_in = t.data('animation_in');
		var animation_out = t.data('animation_out');
		var margin = 10;
		var auto_height = 0;
		var thumbs_event = t.data('thumbs_event');
		if(typeof thumbs_event === 'undefined'){
			thumbs_event = 'click';
		}
		if (t.hasClass('no_spaces')) {
			margin = 5;
			auto_height = 1;
		}

		t.find('.fs-image-nav .fs-thumb').each(function(i) {
				$(this).addClass( 'item'+i );
					if (t.hasClass('old-style')) {
						$(this).click(function() {
							t.find('.fslides').trigger( 'slideTo', [i, 0, true] );
						return false;
						});
					}
			});

			t.find('.fs-image-nav .fs-thumb.item0').addClass( 'active' );

			var carou_items = 6;
			if ($(window).width() < 768) {
					carou_items = 5;
			}
			if ($(window).width() < 568) {
					carou_items = 4;
			}

			if ($(window).width() < 480 ) {
					carou_items = 2;
			}

			if ($('body').hasClass('rtl')) {
				rtl = true;
			}

			if (!t.hasClass('old-style')) {
				var owl = t.find('.fslides');
				var slides_count = owl.children('.fslide').length;
				if (slides_count == 1) {
					t.addClass('fs-with-one-slide');
				}
				if (slides_count > 1) {
				owl.owlCarousel({
					items: 1,
					baseClass: 'mom-carousel',
					rtl: rtl,
					autoplay:autoplay,
					autoplayTimeout:timeout,
					autoplayHoverPause : true,
					loop: true,
		            animateOut: animation_out,
		            animateIn: animation_in,
		            smartSpeed:1000,
		            autoHeight: true,
				});
			t.find('.fslides').imagesLoaded( function() {
				t.find('.fs-image-nav .fs-thumbs').owlCarousel({
					items: items,
					baseClass: 'mom-carousel',
					rtl: rtl,
					loop: true,
		            margin : margin,
				});
				//var thumb_height = t.find('.fs-image-nav .fs-thumbs .fs-thumb').eq(0).css('height');
				//t.find('.fs-image-nav .fs-thumbs').css('max-height', thumb_height);


				owl.on('changed.owl.carousel', function(event) {
    					var pos = $(event.target).find(".fslide").eq(event.item.index).data('i');
						//console.log(pos);
						$('.fc-nav-'+rndn+' .fs-thumb').removeClass( 'active' );
						$('.fc-nav-'+rndn+' .fs-thumb.item'+pos).addClass( 'active' );
						var page = Math.floor( pos / items );
						t.find('.fs-image-nav .fs-thumbs').trigger( 'to.owl.carousel', page );
				});


				t.find('.fs-image-nav .fs-thumb').on(thumbs_event, function() {
					var i = $(this).data('i');
 					owl.trigger('to.owl.carousel', [i]);
				});

				t.find('.fs-image-nav .fs-prev, .fsd-prev').click(function() {
 					owl.trigger('prev.owl.carousel');
				});

				t.find('.fs-image-nav .fs-next, .fsd-next').click(function() {
 					owl.trigger('next.owl.carousel');
				});
			});
			}
			} else {
			t.find('.fslides').carouFredSel({
					circular: true,
                    responsive: true,
					swipe: {
						onTouch: true,
						fx : 'scroll'
					},
					items: 1,
					auto: {
                                             play: autoplay,
                                             duration: speed,
                                             timeoutDuration: timeout,
                                             },
					prev: '.fc-nav-'+rndn+' .fs-prev, .fs-dnav-'+rndn+' span.fsd-prev',
					next: '.fc-nav-'+rndn+' .fs-next, .fs-dnav-'+rndn+' span.fsd-next',
					pagination: '.fs-nav-'+rndn,
					scroll: {
						fx: animation,
                                                  duration : speed,
                                                easing  : easing,
						pauseOnHover : true,
                                        	onBefore: function() {
							var pos = $(this).triggerHandler( 'currentPosition' );
							$('.fc-nav-'+rndn+' div').removeClass( 'active' );
							$('.fc-nav-'+rndn+' div.item'+pos).addClass( 'active' );
							var page = Math.floor( pos / carou_items );
							$('.fc-nav-'+rndn+' .fs-thumbs').trigger( 'slideToPage', page );

						},
						onAfter: function() {
						}

					}
			});

			t.find('.fs-image-nav .fs-thumbs').carouFredSel({
						auto: false,
						circular:true,
                                        responsive: true,
						swipe: {
							onTouch: true
						},
						items: carou_items,
						scroll: {
							items:carou_items,
						}
			});
		}

		} //if not the post gallery

	});
}
// Delete share caches on click
	jQuery(".php-share .ss-icon a").click( function(e){
		var t = jQuery(this);
		var id = t.parent().parent().data('id');
			jQuery.ajax({
			    type: "post",
			    url: momAjaxL.url,
			    dataType: 'html',
			    data: "action=mcscoc&nonce="+momAjaxL.nonce+"&id="+id,
			    beforeSend: function() {},
			    success: function(){}
			});
	});

}); // end of the file

// ad clicks
jQuery(document).ready(function($) {

	jQuery(".mom-e3lan").click( function(e){
		t = jQuery(this);
		id = t.data('id');
			jQuery.ajax({
			    type: "post",
			    url: momAjaxL.url,
			    dataType: 'html',
			    data: "action=mom_mom_adclicks&nonce="+momAjaxL.nonce+"&id="+id,
			    beforeSend: function() {},
			    success: function(){}
			});
	});
});


// Momizat User rate
jQuery(document).ready(function(e){e(".mom_user_rate").mousemove(function(t){var r=e(this).data("style"),s=e(this).offset(),a=t.pageX-s.left;if(t.pageY-s.top,!e(this).hasClass("rated")){var o=a/parseFloat(e(this).width())*100;i=Math.round(o),i>100&&(i=100),n=(i/20).toFixed(1),"bars"===r?(e(this).find(".ub-inner").css({width:i+"%"}),e(this).find(".ub-inner").find("span").text(i+"%")):"circles"===r||e(this).children("span").css({width:i+1+"%"}),e(this).hasClass("star-rating")&&e(this).parent().find(".yr").text(n+"/5")}}),e(".mom_user_rate, .mom_user_rate_cr").hover(function(){e(this).hasClass("rated")||(e(".review-footer .mom_user_rate_title").find(".user_rate").hide(),e(".review-footer .mom_user_rate_title").find(".your_rate").show())},function(){e(this).hasClass("rated")||(e(".mom_user_rate_title").find(".user_rate").show(),e(".mom_user_rate_title").find(".your_rate").hide())}),e(".mom_user_rate").click(function(){stars=jQuery(this),post_id=stars.data("post_id"),style=stars.data("style"),score=0,"stars"===style&&(score=parseFloat(stars.children("span").width())/parseFloat(e(this).width())*100),"bars"===style&&(score=parseFloat(stars.children(".ub-inner").width())/parseFloat(e(this).width())*100),score=Math.round(score),vc=stars.data("votes_count"),e(this).hasClass("rated")||jQuery.ajax({type:"post",url:momAjaxL.url,data:"action=user-rate&nonce="+momAjaxL.nonce+"&user_rate=&post_id="+post_id+"&user_rate_score="+score,success:function(t){"already"!=t&&(stars.addClass("rated"),e(".review-footer .mom_user_rate_title").find(".user_rate").hide(),e(".review-footer .mom_user_rate_title").find(".your_rate").show(),e(".review-footer .total-votes").find(".tv-count").text(vc+1))}})}),e(".mom-reveiw-system").length&&e(".urc-value").knob({displayInput:!1,change:function(t){e(".user-rate-circle").find(".cru-num").text(t)},release:function(t){circle=jQuery(".user-rate-circle .mom_user_rate_cr"),post_id=circle.data("post_id"),style=circle.data("style"),score=t,vc=circle.data("votes_count"),jQuery.ajax({type:"post",url:momAjaxL.url,data:"action=user-rate&nonce="+momAjaxL.nonce+"&user_rate=&post_id="+post_id+"&user_rate_score="+score,success:function(t){"already"!=t&&(circle.addClass("rated"),e(".review-footer .mom_user_rate_title").find(".user_rate").hide(),e(".review-footer .mom_user_rate_title").find(".your_rate").show(),e(".review-footer .total-votes").find(".tv-count").text(vc+1))}})}})});


// Momizat ajax
jQuery(document).ready(function() {
    jQuery(".mom-search-form input.sf").on("keyup", function() {
        return sf = jQuery(this), term = sf.val(), term.length > 2 ? setTimeout(function() {
            jQuery.ajax({
                type: "post",
                url: momAjaxL.url,
                dataType: "html",
                data: "action=mom_ajaxsearch&nonce=" + momAjaxL.nonce + "&term=" + term,
                beforeSend: function() {
                    sf.parent().parent().find(".sf-loading").fadeIn()
                },
                success: function(e) {
                    "" !== sf.val() ? (sf.parent().parent().next(".ajax_search_results").html(e), "" !== e ? sf.parent().parent().next(".ajax_search_results").append('<footer class="show_all_results"><a href="' + momAjaxL.homeUrl + "/?s=" + term + '">' + momAjaxL.viewAll + '<i class="fa-icon-long-arrow-right"></i></a></footer>') : (sf.parent().parent().next(".ajax_search_results").find("show_all_results").remove(), sf.parent().parent().next(".ajax_search_results").html('<span class="sw-not_found">' + momAjaxL.noResults + "</span>"))) : sf.parent().parent().next(".ajax_search_results").html(""), sf.parent().parent().find(".sf-loading").fadeOut()
                }
            })
        }, 300) : setTimeout(function() {
            jQuery.ajax({
                type: "post",
                url: momAjaxL.url,
                dataType: "html",
                data: "action=mom_ajaxsearch&nonce=" + momAjaxL.nonce + "&term=" + term,
                success: function() {
                    "" === sf.val() && sf.parent().parent().next(".ajax_search_results").html("")
                }
            })
        }, 300), !1
    })
}), jQuery(document).ready(function(e) {
    offset = "", jQuery("a.show-more-posts").click(function(a) {
        a.preventDefault();
        var t = e(this);
        style = t.data("style"), share = t.data("share"), count = t.data("count"), offset = t.data("offset"), display = t.data("display"), category = t.data("category"), tag = t.data("tag"), sort = t.data("sort"), orderby = t.data("orderby"), format = t.data("format"), excerpt_length = t.data("excerpt_length"), load_more_count = t.data("load_more_count"), exclude_cats = t.data("exclude_cats"), jQuery.ajax({
            type: "post",
            url: momAjaxL.url,
            dataType: "html",
            data: "action=mom_loadMore&nonce=" + momAjaxL.nonce + "&display=" + display + "&category=" + category + "&tag=" + tag + "&number_of_posts=" + count + "&sort=" + sort + "&orderby=" + orderby + "&offset=" + offset + "&format=" + format + "&excerpt_length=" + excerpt_length + "&style=" + style + "&share=" + share + "&load_more_count=" + load_more_count+ "&exclude_cats=" + exclude_cats,
            beforeSend: function() {
                t.find("i").addClass("fa-spin")
            },
            success: function(e) {
							if (style == 'g') {
								t.prev('.posts-grid').append(e);
							} else {
								t.before(e);
							}
							 t.find("i").removeClass("fa-spin"), "" === e && t.text(momAjaxL.nomore)
            }
        }), t.data("offset", offset + load_more_count), console.log(offset)
    })
}), jQuery(document).ready(function() {
    offset = "", offset_rest = "", jQuery(".nb-footer a.show-more-ajax").click(function(e) {
        e.preventDefault(), bt = jQuery(this), where = bt.parent().prev(), nbs = bt.data("nbs"), nop = bt.data("number_of_posts"), offset = bt.data("offset"), offset_rest = offset + 1, post_type = bt.data("post_type"), display = bt.data("display"), category = bt.data("category"), tag = bt.data("tag"), sort = bt.data("sort"), orderby = bt.data("orderby"), exclude_cats = bt.data("exclude_cats"), format = "", image_size = "", excerpt_length = "", "news_list" === nbs && (format = bt.data("format"), image_size = bt.data("image_size"), excerpt_length = bt.data("excerpt_length")), jQuery.ajax({
            type: "post",
            url: momAjaxL.url,
            dataType: "html",
            data: "action=nbsm&nonce=" + momAjaxL.nonce + "&display=" + display + "&category=" + category + "&tag=" + tag + "&nbs=" + nbs + "&number_of_posts=" + nop + "&sort=" + sort + "&orderby=" + orderby + "&offset=" + offset + "&offset_all=" + offset_rest + "&format=" + format + "&image_size=" + image_size + "&excerpt_length=" + excerpt_length + "&post_type=" + post_type+ "&exclude_cats=" + exclude_cats,
            beforeSend: function() {
                where.append('<i class="nb-load"></i>')
            },
            success: function(e) {
                "" == e && bt.parent().append('<a class="nomoreposts">' + momAjaxL.nomore + "</a>").hide().fadeIn(), "" !== e && where.html(e), where.find(".nb-load").remove()
            },
            complete: function() {}
        }), bt.data("offset", offset + (nop + 1))
    })
}), jQuery(document).ready(function(e) {
    jQuery(".weather-form").submit(function() {
        return form = jQuery(this), city = form.find("input").val(), lang = form.find("input").data("lang"), units = form.find("input").data("units"), jQuery.ajax({
            type: "post",
            url: momAjaxL.url,
            dataType: "html",
            data: "action=mom_ajaxweather&nonce=" + momAjaxL.nonce + "&city=" + city + "&lang=" + lang + "&units=" + units,
            beforeSend: function() {
                form.find(".sf-loading").fadeIn()
            },
            success: function(a) {
                "" !== city && ("" !== a ? (form.nextAll(".weather-widget").html(a).hide().fadeIn(), form.next(".message-box").fadeOut()) : (form.next(".message-box").remove(), form.after('<span class="message-box error">' + momAjaxL.werror + '<i class="brankic-icon-error"></i></span>'))), form.find(".sf-loading").fadeOut(), e(".message-box i").on("click", function() {
                    e(this).parent().fadeOut()
                })
            }
        }), !1
    })
});

// email valid
function mom_isValidEmailAddress(emailAddress) {
    var pattern = new RegExp(/^(("[\w-+\s]+")|([\w-+]+(?:\.[\w-+]+)*)|("[\w-+\s]+")([\w-+]+(?:\.[\w-+]+)*))(@((?:[\w-+]+\.)*\w[\w-+]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$)|(@\[?((25[0-5]\.|2[0-4][\d]\.|1[\d]{2}\.|[\d]{1,2}\.))((25[0-5]|2[0-4][\d]|1[\d]{2}|[\d]{1,2})\.){2}(25[0-5]|2[0-4][\d]|1[\d]{2}|[\d]{1,2})\]?$)/i);
    return pattern.test(emailAddress);
};

/*!
 * imagesLoaded PACKAGED v3.1.8
 * JavaScript is all like "You images are done yet or what?"
 * MIT License
 */

(function(){function e(){}function t(e,t){for(var n=e.length;n--;)if(e[n].listener===t)return n;return-1}function n(e){return function(){return this[e].apply(this,arguments)}}var i=e.prototype,r=this,o=r.EventEmitter;i.getListeners=function(e){var t,n,i=this._getEvents();if("object"==typeof e){t={};for(n in i)i.hasOwnProperty(n)&&e.test(n)&&(t[n]=i[n])}else t=i[e]||(i[e]=[]);return t},i.flattenListeners=function(e){var t,n=[];for(t=0;e.length>t;t+=1)n.push(e[t].listener);return n},i.getListenersAsObject=function(e){var t,n=this.getListeners(e);return n instanceof Array&&(t={},t[e]=n),t||n},i.addListener=function(e,n){var i,r=this.getListenersAsObject(e),o="object"==typeof n;for(i in r)r.hasOwnProperty(i)&&-1===t(r[i],n)&&r[i].push(o?n:{listener:n,once:!1});return this},i.on=n("addListener"),i.addOnceListener=function(e,t){return this.addListener(e,{listener:t,once:!0})},i.once=n("addOnceListener"),i.defineEvent=function(e){return this.getListeners(e),this},i.defineEvents=function(e){for(var t=0;e.length>t;t+=1)this.defineEvent(e[t]);return this},i.removeListener=function(e,n){var i,r,o=this.getListenersAsObject(e);for(r in o)o.hasOwnProperty(r)&&(i=t(o[r],n),-1!==i&&o[r].splice(i,1));return this},i.off=n("removeListener"),i.addListeners=function(e,t){return this.manipulateListeners(!1,e,t)},i.removeListeners=function(e,t){return this.manipulateListeners(!0,e,t)},i.manipulateListeners=function(e,t,n){var i,r,o=e?this.removeListener:this.addListener,s=e?this.removeListeners:this.addListeners;if("object"!=typeof t||t instanceof RegExp)for(i=n.length;i--;)o.call(this,t,n[i]);else for(i in t)t.hasOwnProperty(i)&&(r=t[i])&&("function"==typeof r?o.call(this,i,r):s.call(this,i,r));return this},i.removeEvent=function(e){var t,n=typeof e,i=this._getEvents();if("string"===n)delete i[e];else if("object"===n)for(t in i)i.hasOwnProperty(t)&&e.test(t)&&delete i[t];else delete this._events;return this},i.removeAllListeners=n("removeEvent"),i.emitEvent=function(e,t){var n,i,r,o,s=this.getListenersAsObject(e);for(r in s)if(s.hasOwnProperty(r))for(i=s[r].length;i--;)n=s[r][i],n.once===!0&&this.removeListener(e,n.listener),o=n.listener.apply(this,t||[]),o===this._getOnceReturnValue()&&this.removeListener(e,n.listener);return this},i.trigger=n("emitEvent"),i.emit=function(e){var t=Array.prototype.slice.call(arguments,1);return this.emitEvent(e,t)},i.setOnceReturnValue=function(e){return this._onceReturnValue=e,this},i._getOnceReturnValue=function(){return this.hasOwnProperty("_onceReturnValue")?this._onceReturnValue:!0},i._getEvents=function(){return this._events||(this._events={})},e.noConflict=function(){return r.EventEmitter=o,e},"function"==typeof define&&define.amd?define("eventEmitter/EventEmitter",[],function(){return e}):"object"==typeof module&&module.exports?module.exports=e:this.EventEmitter=e}).call(this),function(e){function t(t){var n=e.event;return n.target=n.target||n.srcElement||t,n}var n=document.documentElement,i=function(){};n.addEventListener?i=function(e,t,n){e.addEventListener(t,n,!1)}:n.attachEvent&&(i=function(e,n,i){e[n+i]=i.handleEvent?function(){var n=t(e);i.handleEvent.call(i,n)}:function(){var n=t(e);i.call(e,n)},e.attachEvent("on"+n,e[n+i])});var r=function(){};n.removeEventListener?r=function(e,t,n){e.removeEventListener(t,n,!1)}:n.detachEvent&&(r=function(e,t,n){e.detachEvent("on"+t,e[t+n]);try{delete e[t+n]}catch(i){e[t+n]=void 0}});var o={bind:i,unbind:r};"function"==typeof define&&define.amd?define("eventie/eventie",o):e.eventie=o}(this),function(e,t){"function"==typeof define&&define.amd?define(["eventEmitter/EventEmitter","eventie/eventie"],function(n,i){return t(e,n,i)}):"object"==typeof exports?module.exports=t(e,require("wolfy87-eventemitter"),require("eventie")):e.imagesLoaded=t(e,e.EventEmitter,e.eventie)}(window,function(e,t,n){function i(e,t){for(var n in t)e[n]=t[n];return e}function r(e){return"[object Array]"===d.call(e)}function o(e){var t=[];if(r(e))t=e;else if("number"==typeof e.length)for(var n=0,i=e.length;i>n;n++)t.push(e[n]);else t.push(e);return t}function s(e,t,n){if(!(this instanceof s))return new s(e,t);"string"==typeof e&&(e=document.querySelectorAll(e)),this.elements=o(e),this.options=i({},this.options),"function"==typeof t?n=t:i(this.options,t),n&&this.on("always",n),this.getImages(),a&&(this.jqDeferred=new a.Deferred);var r=this;setTimeout(function(){r.check()})}function f(e){this.img=e}function c(e){this.src=e,v[e]=this}var a=e.jQuery,u=e.console,h=u!==void 0,d=Object.prototype.toString;s.prototype=new t,s.prototype.options={},s.prototype.getImages=function(){this.images=[];for(var e=0,t=this.elements.length;t>e;e++){var n=this.elements[e];"IMG"===n.nodeName&&this.addImage(n);var i=n.nodeType;if(i&&(1===i||9===i||11===i))for(var r=n.querySelectorAll("img"),o=0,s=r.length;s>o;o++){var f=r[o];this.addImage(f)}}},s.prototype.addImage=function(e){var t=new f(e);this.images.push(t)},s.prototype.check=function(){function e(e,r){return t.options.debug&&h&&u.log("confirm",e,r),t.progress(e),n++,n===i&&t.complete(),!0}var t=this,n=0,i=this.images.length;if(this.hasAnyBroken=!1,!i)return this.complete(),void 0;for(var r=0;i>r;r++){var o=this.images[r];o.on("confirm",e),o.check()}},s.prototype.progress=function(e){this.hasAnyBroken=this.hasAnyBroken||!e.isLoaded;var t=this;setTimeout(function(){t.emit("progress",t,e),t.jqDeferred&&t.jqDeferred.notify&&t.jqDeferred.notify(t,e)})},s.prototype.complete=function(){var e=this.hasAnyBroken?"fail":"done";this.isComplete=!0;var t=this;setTimeout(function(){if(t.emit(e,t),t.emit("always",t),t.jqDeferred){var n=t.hasAnyBroken?"reject":"resolve";t.jqDeferred[n](t)}})},a&&(a.fn.imagesLoaded=function(e,t){var n=new s(this,e,t);return n.jqDeferred.promise(a(this))}),f.prototype=new t,f.prototype.check=function(){var e=v[this.img.src]||new c(this.img.src);if(e.isConfirmed)return this.confirm(e.isLoaded,"cached was confirmed"),void 0;if(this.img.complete&&void 0!==this.img.naturalWidth)return this.confirm(0!==this.img.naturalWidth,"naturalWidth"),void 0;var t=this;e.on("confirm",function(e,n){return t.confirm(e.isLoaded,n),!0}),e.check()},f.prototype.confirm=function(e,t){this.isLoaded=e,this.emit("confirm",this,t)};var v={};return c.prototype=new t,c.prototype.check=function(){if(!this.isChecked){var e=new Image;n.bind(e,"load",this),n.bind(e,"error",this),e.src=this.src,this.isChecked=!0}},c.prototype.handleEvent=function(e){var t="on"+e.type;this[t]&&this[t](e)},c.prototype.onload=function(e){this.confirm(!0,"onload"),this.unbindProxyEvents(e)},c.prototype.onerror=function(e){this.confirm(!1,"onerror"),this.unbindProxyEvents(e)},c.prototype.confirm=function(e,t){this.isConfirmed=!0,this.isLoaded=e,this.emit("confirm",this,t)},c.prototype.unbindProxyEvents=function(e){n.unbind(e.target,"load",this),n.unbind(e.target,"error",this)},s});

/*
    JQuery Advanced News Ticker 1.0.11 (20/02/14)
    created by risq
    website (docs & demos) : http://risq.github.io/jquery-advanced-news-ticker/
*/
(function(b,k,l,m){function g(a,f){this.element=a;this.$el=b(a);this.options=b.extend({},c,f);this._defaults=c;this._name=d;this.moveInterval;this.moving=this.paused=this.state=0;(this.$el.is("ul")||this.$el.is("ol"))&&this.init()}var d="newsTicker",c={row_height:20,max_rows:3,speed:400,duration:2500,direction:"up",autostart:1,pauseOnHover:1,nextButton:null,prevButton:null,startButton:null,stopButton:null,hasMoved:function(){},movingUp:function(){},movingDown:function(){},start:function(){},stop:function(){},
pause:function(){},unpause:function(){}};g.prototype={init:function(){this.$el.height(this.options.row_height*this.options.max_rows).css({overflow:"hidden"});this.checkSpeed();this.options.nextButton&&"undefined"!==typeof this.options.nextButton[0]&&this.options.nextButton.click(function(a){this.moveNext();this.resetInterval()}.bind(this));this.options.prevButton&&"undefined"!==typeof this.options.prevButton[0]&&this.options.prevButton.click(function(a){this.movePrev();this.resetInterval()}.bind(this));
this.options.stopButton&&"undefined"!==typeof this.options.stopButton[0]&&this.options.stopButton.click(function(a){this.stop()}.bind(this));this.options.startButton&&"undefined"!==typeof this.options.startButton[0]&&this.options.startButton.click(function(a){this.start()}.bind(this));this.options.pauseOnHover&&this.$el.hover(function(){this.state&&this.pause()}.bind(this),function(){this.state&&this.unpause()}.bind(this));this.options.autostart&&this.start()},start:function(){this.state||(this.state=
1,this.resetInterval(),this.options.start())},stop:function(){this.state&&(clearInterval(this.moveInterval),this.state=0,this.options.stop())},resetInterval:function(){this.state&&(clearInterval(this.moveInterval),this.moveInterval=setInterval(function(){this.move()}.bind(this),this.options.duration))},move:function(){this.paused||this.moveNext()},moveNext:function(){"down"===this.options.direction?this.moveDown():"up"===this.options.direction&&this.moveUp()},movePrev:function(){"down"===this.options.direction?
this.moveUp():"up"===this.options.direction&&this.moveDown()},pause:function(){this.paused||(this.paused=1);this.options.pause()},unpause:function(){this.paused&&(this.paused=0);this.options.unpause()},moveDown:function(){this.moving||(this.moving=1,this.options.movingDown(),this.$el.children("li:last").detach().prependTo(this.$el).css("marginTop","-"+this.options.row_height+"px").animate({marginTop:"0px"},this.options.speed,function(){this.moving=0;this.options.hasMoved()}.bind(this)))},moveUp:function(){if(!this.moving){this.moving=
1;this.options.movingUp();var a=this.$el.children("li:first");a.animate({marginTop:"-"+this.options.row_height+"px"},this.options.speed,function(){a.detach().css("marginTop","0").appendTo(this.$el);this.moving=0;this.options.hasMoved()}.bind(this))}},updateOption:function(a,b){"undefined"!==typeof this.options[a]&&(this.options[a]=b,"duration"==a||"speed"==a)&&(this.checkSpeed(),this.resetInterval())},add:function(a){this.$el.append(b("<li>").html(a))},getState:function(){return paused?2:this.state},
checkSpeed:function(){this.options.duration<this.options.speed+25&&(this.options.speed=this.options.duration-25)},destroy:function(){this._destroy()}};b.fn[d]=function(a){var f=arguments;return this.each(function(){var c=b(this),e=b.data(this,"plugin_"+d),h="object"===typeof a&&a;e||c.data("plugin_"+d,e=new g(this,h));"string"===typeof a&&e[a].apply(e,Array.prototype.slice.call(f,1))})}})(jQuery,window,document);
/*!
 * jquery.okayNav.js 2.0.4 (https://github.com/VPenkov/okayNav)
 * Author: Vergil Penkov (http://vergilpenkov.com/)
 * MIT license: https://opensource.org/licenses/MIT
 */
!function(e){"function"==typeof define&&define.amd?define(["jquery"],e):"object"==typeof module&&module.exports?module.exports=function(n,i){return void 0===i&&(i="undefined"!=typeof window?require("jquery"):require("jquery")(n)),e(i),i}:e(jQuery)}(function(e){function n(n,i){self=this,this.options=e.extend({},s,i),self.options=this.options,self.navigation=e(n),self.document=e(document),self.window=e(window),""==this.options.parent?this.options.parent=self.navigation.parent():"",self.nav_open=!1,self.parent_full_width=0,self.radCoef=180/Math.PI,self.sTouch={x:0,y:0},self.cTouch={x:0,y:0},self.sTime=0,self.nav_position=0,self.percent_open=0,self.nav_moving=!1,self.init()}var i="okayNav",s={parent:"",toggle_icon_class:"okayNav__menu-toggle",toggle_icon_content:"<span /><span /><span />",align_right:!0,swipe_enabled:!0,threshold:50,resize_delay:10,beforeOpen:function(){},afterOpen:function(){},beforeClose:function(){},afterClose:function(){},itemHidden:function(){},itemDisplayed:function(){}};n.prototype={init:function(){e("body").addClass("okayNav-loaded"),self.navigation.addClass("okayNav loaded").children("ul").addClass("okayNav__nav--visible"),self.options.align_right?self.navigation.append('<ul class="okayNav__nav--invisible transition-enabled nav-right" />').append('<a href="#" class="'+self.options.toggle_icon_class+' okay-invisible">'+self.options.toggle_icon_content+"</a>"):self.navigation.prepend('<ul class="okayNav__nav--invisible transition-enabled nav-left" />').prepend('<a href="#" class="'+self.options.toggle_icon_class+' okay-invisible">'+self.options.toggle_icon_content+"</a>"),self.nav_visible=self.navigation.children(".okayNav__nav--visible"),self.nav_invisible=self.navigation.children(".okayNav__nav--invisible"),self.toggle_icon=self.navigation.children("."+self.options.toggle_icon_class),self.toggle_icon_width=self.toggle_icon.outerWidth(!0),self.default_width=self.getChildrenWidth(self.navigation),self.parent_full_width=e(self.options.parent).outerWidth(!0),self.last_visible_child_width=0,self.initEvents(),self.nav_visible.contents().filter(function(){return this.nodeType=Node.TEXT_NODE&&/\S/.test(this.nodeValue)===!1}).remove(),1==self.options.swipe_enabled&&self.initSwipeEvents()},initEvents:function(){self.document.on("click.okayNav",function(n){var i=e(n.target);self.nav_open===!0&&0==i.closest(".okayNav").length&&self.closeInvisibleNav(),i.hasClass(self.options.toggle_icon_class)&&(n.preventDefault(),self.toggleInvisibleNav())});var n=self._debounce(function(){self.recalcNav()},self.options.recalc_delay);self.window.on("load.okayNav resize.okayNav",n)},initSwipeEvents:function(){self.document.on("touchstart.okayNav",function(n){if(self.nav_invisible.removeClass("transition-enabled"),1==n.originalEvent.touches.length){var i=n.originalEvent.touches[0];(i.pageX<25&&0==self.options.align_right||i.pageX>e(self.options.parent).outerWidth(!0)-25&&1==self.options.align_right||self.nav_open===!0)&&(self.sTouch.x=self.cTouch.x=i.pageX,self.sTouch.y=self.cTouch.y=i.pageY,self.sTime=Date.now())}}).on("touchmove.okayNav",function(e){var n=e.originalEvent.touches[0];self._triggerMove(n.pageX,n.pageY),self.nav_moving=!0}).on("touchend.okayNav",function(e){self.sTouch={x:0,y:0},self.cTouch={x:0,y:0},self.sTime=0,self.percent_open>100-self.options.threshold?(self.nav_position=0,self.closeInvisibleNav()):1==self.nav_moving&&(self.nav_position=self.nav_invisible.width(),self.openInvisibleNav()),self.nav_moving=!1,self.nav_invisible.addClass("transition-enabled")})},_getDirection:function(e){return self.options.align_right?e>0?-1:1:0>e?-1:1},_triggerMove:function(e,n){self.cTouch.x=e,self.cTouch.y=n;var i=Date.now(),s=self.cTouch.x-self.sTouch.x,l=self.cTouch.y-self.sTouch.y,t=l*l,o=Math.sqrt(s*s+t),a=Math.sqrt(t),f=Math.asin(Math.sin(a/o))*self.radCoef;o/(i-self.sTime);if(self.sTouch.x=e,self.sTouch.y=n,20>f){var v=self._getDirection(s),c=self.nav_position+v*o,r=self.nav_invisible.width(),d=0;0>c?d=-c:c>r&&(d=r-c);var _=r-(self.nav_position+v*o+d),p=_/r*100;self.nav_position+=v*o+d,self.percent_open=p,self.nav_invisible.css("transform","translateX("+(self.options.align_right?1:-1)*p+"%)")}},getParent:function(){return self.options.parent},getVisibleNav:function(){return self.nav_visible},getInvisibleNav:function(){return self.nav_invisible},getNavToggleIcon:function(){return self.toggle_icon},_debounce:function(e,n,i){var s;return function(){var l=this,t=arguments,o=function(){s=null,i||e.apply(l,t)},a=i&&!s;clearTimeout(s),s=setTimeout(o,n),a&&e.apply(l,t)}},openInvisibleNav:function(){self.options.enable_swipe?"":self.options.beforeOpen.call(),self.toggle_icon.addClass("icon--active"),self.nav_invisible.addClass("nav-open"),self.nav_open=!0,self.nav_invisible.css({"-webkit-transform":"translateX(0%)",transform:"translateX(0%)"}),self.options.afterOpen.call()},closeInvisibleNav:function(){self.options.enable_swipe?"":self.options.beforeClose.call(),self.toggle_icon.removeClass("icon--active"),self.nav_invisible.removeClass("nav-open"),self.options.align_right?self.nav_invisible.css({"-webkit-transform":"translateX(100%)",transform:"translateX(100%)"}):self.nav_invisible.css({"-webkit-transform":"translateX(-100%)",transform:"translateX(-100%)"}),self.nav_open=!1,self.options.afterClose.call()},toggleInvisibleNav:function(){self.nav_open?self.closeInvisibleNav():self.openInvisibleNav()},getChildrenWidth:function(n){for(var i=0,s=e(n).children(),l=0;l<s.length;l++)i+=e(s[l]).outerWidth(!0);return i},getVisibleItemCount:function(){return e("li",self.nav_visible).length},getHiddenItemCount:function(){return e("li",self.nav_invisible).length},recalcNav:function(){var n=e(self.options.parent).outerWidth(!0),i=self.getChildrenWidth(self.options.parent),s=self.navigation.outerWidth(!0),l=self.getVisibleItemCount(),t=self.nav_visible.outerWidth(!0)+self.toggle_icon_width,o=i+self.last_visible_child_width+self.toggle_icon_width,a=i-s+self.default_width;return n>a?(self._expandAllItems(),void self.toggle_icon.addClass("okay-invisible")):(l>0&&t>=s&&o>=n&&self._collapseNavItem(),n>o+self.toggle_icon_width+15&&self._expandNavItem(),void(0==self.getHiddenItemCount()?self.toggle_icon.addClass("okay-invisible"):self.toggle_icon.removeClass("okay-invisible")))},_collapseNavItem:function(){var n=e("li:last-child",self.nav_visible);self.last_visible_child_width=n.outerWidth(!0),self.document.trigger("okayNav:collapseItem",n),n.detach().prependTo(self.nav_invisible),self.options.itemHidden.call(),self.recalcNav()},_expandNavItem:function(){var n=e("li:first-child",self.nav_invisible);self.document.trigger("okayNav:expandItem",n),n.detach().appendTo(self.nav_visible),self.options.itemDisplayed.call()},_expandAllItems:function(){e("li",self.nav_invisible).detach().appendTo(self.nav_visible),self.options.itemDisplayed.call()},_collapseAllItems:function(){e("li",self.nav_visible).detach().appendTo(self.nav_invisible),self.options.itemHidden.call()},destroy:function(){e("li",self.nav_invisible).appendTo(self.nav_visible),self.nav_invisible.remove(),self.nav_visible.removeClass("okayNav__nav--visible"),self.toggle_icon.remove(),self.document.unbind(".okayNav"),self.window.unbind(".okayNav")}},e.fn[i]=function(s){var l=arguments;if(void 0===s||"object"==typeof s)return this.each(function(){e.data(this,"plugin_"+i)||e.data(this,"plugin_"+i,new n(this,s))});if("string"==typeof s&&"_"!==s[0]&&"init"!==s){var t;return this.each(function(){var o=e.data(this,"plugin_"+i);o instanceof n&&"function"==typeof o[s]&&(t=o[s].apply(o,Array.prototype.slice.call(l,1))),"destroy"===s&&e.data(this,"plugin_"+i,null)}),void 0!==t?t:this}}});
