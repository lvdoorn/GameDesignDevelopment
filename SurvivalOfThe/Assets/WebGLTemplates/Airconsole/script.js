navigator.vibrate = (navigator.vibrate ||
                         navigator.webkitVibrate ||
                         navigator.mozVibrate ||
                         navigator.msVibrate);

var airconsole;
var state = "waiting";
/**
  * jQuery helpers for touch vs mouse interaction
  * Only one of ontouchstart/end and onmousedown/up events will be triggered
  */
(function ($) {
  $.fn.touchDown = function (callback) {
    this.bind("touchstart", function (event) {
	  event.stopPropagation();
	  event.preventDefault();
	  callback.call(this, event);
	});
    this.bind("mousedown", function (event) {
	  event.stopPropagation();
	  event.preventDefault();
	  callback.call(this, event);
	});   
    return this;
  };
  $.fn.touchUp = function (callback) {
    this.bind("touchend", function (event) {
	  event.stopPropagation();
	  event.preventDefault();
	  callback.call(this, event);
	});
    this.bind("mouseup", function (event) {
	  event.stopPropagation();
	  event.preventDefault();
	  callback.call(this, event);
	});   
    return this;
  };
})(jQuery);
/**
  * Sets up the communication to the screen.
  */
$(function() {
  $("#please_wait").hide();
  init();
  airconsole = new AirConsole({"orientation": "landscape"});
  airconsole.onMessage = function (from, data) {
	if (from == AirConsole.SCREEN) {
		if (data.vibrate) {
		  if (navigator.vibrate) {
			navigator.vibrate(data.vibrate);
		  }
		}
		if (data.addItem) {
		  var elem = $("#inventory_item_" + data.slot);
		  elem.css("background-image", "url('" + data.addItem + ".png')");
		  if (navigator.vibrate) {
			navigator.vibrate(1000);
		  }
		}
		if (data.removeItem) {
		  var elem = $("#inventory_item_" + data.slot);
		  elem.css("background-image", "url('btn_item.png')");
		}
		if (data.setColor) {
		  $(".color").css("background-color", data.setColor);
		}
	}
  };
  airconsole.onCustomDeviceStateChange = function(from, data) {
	if (from == AirConsole.SCREEN) {
		if (data.state == "Play") {
		  state = "playing";
		}
		if (data.state == "Vote") {
		  state = "voting";
		}
		if (data.state == "Join") {
		  state = "joining";
		}
		if (data.state == "Wait") {
		  state = "waiting";
		}
		if (data.action1) {
			$("#action1lbl").html(data.action1);
			$("#action1").show();
		} else {
			$("#action1").hide();
		}
		if (data.action2) {
			$("#action2lbl").html(data.action2);
			$("#action2").show();
		} else {
			$("#action2").hide();
		}
		if (data.text) {
		  $("#please_wait > p").html(data.text);
		}
	}
	updateVisibility();
  };
  airconsole.onReady = function () {
    updateVisibility();
  };
  // wrap in RateLimiter
  airconsole = new RateLimiter(airconsole);
  preloadImages();
});
/**
  * Initialize all controls
  */
function init() {
  $("#action1").touchDown(function() {
	action1();
  });
  $("#action2").touchDown(function() {
	action2();
  });
  $("#inventory_item_0").touchDown(function() {
	itemUsed(0);
  });
  $("#inventory_item_1").touchDown(function() {
	itemUsed(1);
  });
  $("#inventory_item_2").touchDown(function() {
	itemUsed(2);
  });
  $("#inventory_item_3").touchDown(function() {
	itemUsed(3);
  });
  $(".focus_button").touchDown(function() {
	requestFocus();
  });
  $(".arrow").touchUp(function() {
	move('S');
  });
  $(".arrowL").touchDown(function() {
	move('L');
  });
  $(".arrowR").touchDown(function() {
	move('R');
  });
  $(".arrowU").touchDown(function() {
	move('U');
  });
  $(".arrowD").touchDown(function() {
	move('D');
  });
}
/**
  * Load all used images to avoid flickering later
  */
function preloadImages() {
  var images = [
    'btn_action_active.png',
	'btn_focus_active.png',
    'btn_up_active.png',
    'btn_right_active.png',
    'btn_left_active.png',
    'btn_down_active.png',
    'prybar.png',
    'machete.png',
    'sample_dna.png',
    'pickaxe.png',
    'fuel.png',
    'fire_extinguisher.png',
    'dynamite.png',
    'dna_sampler.png'
  ];
  $(images).each(function() {
    $('<img/>')[0].src = this;
    (new Image()).src = this;
  });
}

function updateVisibility() {
  if (state == "waiting") {
	$("#please_wait").show();
	$("#left_side").hide();
	$("#right_side").hide();
  } else {
	$("#please_wait").hide();
	$("#left_side").show();
	$("#right_side").show();
  }
}

/**
  * Tells the screen to move the paddle of this player.
  * @param dir
  */
function move(dir)
{
  //airconsole.message(AirConsole.SCREEN, { move: amount })
  airconsole.message(AirConsole.SCREEN, { direction: dir });
  $(".arrow").removeClass("active");
  if (dir != 'S') {
    $(".arrow" + dir).addClass("active");
  }
}
function action1()
{  
  if(state === "joining") {
    airconsole.message(AirConsole.SCREEN, { start: 1 });
  }
  if (state === "playing") {
    airconsole.message(AirConsole.SCREEN, { action: 1 });
  }
  if (state === "voting") {
    airconsole.message(AirConsole.SCREEN, { vote: 1 });
  }
  
  $("#action1").addClass("active");
  setTimeout(function() {
    $("#action1").removeClass("active");
  }, 250);
}
function action2()
{  
  if (state === "playing") {
    airconsole.message(AirConsole.SCREEN, { action: 2 });
  }
  if (state === "voting") {
    airconsole.message(AirConsole.SCREEN, { vote: 2 });
  }
  
  $("#action2").addClass("active");
  setTimeout(function() {
    $("#action2").removeClass("active");
  }, 250);
}

function itemUsed(id)
{
  airconsole.message(AirConsole.SCREEN, { itemUsed: id });
  
  var item = $("#inventory_item_" + id);
  item.stop(true, true).fadeTo(1, 0.3).fadeTo(1000, 1.0);
}

function requestFocus()
{
  airconsole.message(AirConsole.SCREEN, { focus: 1 });

  $(".focus_button").addClass("active");
  setTimeout(function() {
    $(".focus_button").removeClass("active");
  }, 250);
}

