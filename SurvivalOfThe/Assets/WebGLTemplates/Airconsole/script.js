navigator.vibrate = (navigator.vibrate ||
                         navigator.webkitVibrate ||
                         navigator.mozVibrate ||
                         navigator.msVibrate);

var airconsole;
var state = "waiting";

/**
  * Sets up the communication to the screen.
  */
$(function() {
  $("#please_wait").hide();
  airconsole = new AirConsole({"orientation": "landscape"});
  airconsole.onMessage = function (from, data) {
    if (from == AirConsole.SCREEN && data.vibrate) {
	  if (navigator.vibrate) {
		navigator.vibrate(data.vibrate);
	  }
    }
    if (from == AirConsole.SCREEN && data.addItem) {
      var elem = $("#inventory_item_" + data.slot);
      elem.css("background-image", "url('" + data.addItem + ".png')");
	  if (navigator.vibrate) {
		navigator.vibrate(1000);
	  }
    }
    if (from == AirConsole.SCREEN && data.removeItem) {
      var elem = $("#inventory_item_" + data.slot);
      elem.css("background-image", "url('btn_item.png')");
    }
  };
  airconsole.onCustomDeviceStateChange = function(from, data) {
	if (from == AirConsole.SCREEN && data.state == "Play") {
      state = "playing";
    }
    if (from == AirConsole.SCREEN && data.state == "Vote") {
      state = "voting";
    }
    if (from == AirConsole.SCREEN && data.state == "Join") {
      state = "joining";
    }
    if (from == AirConsole.SCREEN && data.state == "Wait") {
      state = "waiting";
    }
	if (from == AirConsole.SCREEN && data.action1 !== undefined) {
      if (data.action1 !== null) {
		$("#action1lbl").html(data.action1);
		$("#action1").show();
	  } else {
		$("#action1").hide();
	  }
    }
	if (from == AirConsole.SCREEN && data.action2 !== undefined) {
      if (data.action2 !== null) {
		$("#action2lbl").html(data.action2);
		$("#action2").show();
	  } else {
		$("#action2").hide();
	  }
    }
	if (from == AirConsole.SCREEN && data.text !== undefined) {
      $("#please_wait > p").html(data.text);
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

