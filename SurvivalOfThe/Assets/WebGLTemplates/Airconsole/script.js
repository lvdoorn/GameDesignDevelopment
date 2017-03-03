navigator.vibrate = (navigator.vibrate ||
                         navigator.webkitVibrate ||
                         navigator.mozVibrate ||
                         navigator.msVibrate);

var airconsole;
var state = "joining";

/**
  * Sets up the communication to the screen.
  */
$(function() {
  $("#please_wait").hide();
  airconsole = new AirConsole({"orientation": "landscape"});
  airconsole.onMessage = function (from, data)
  {
    if (from == AirConsole.SCREEN && data.vibrate)
    {
      navigator.vibrate(data.vibrate);
    }
    if (from == AirConsole.SCREEN && data.addItem)
    {
      var elem = document.getElementById("inventory_item_" + data.slot);
      elem.style.backgroundImage = "url('" + data.addItem + ".png')";
      navigator.vibrate(1000);
    }
    if (from == AirConsole.SCREEN && data.removeItem) {
      var elem = document.getElementById("inventory_item_" + data.slot);
      elem.style.backgroundImage = "url('btn_item.png')";
    }

    if (from == AirConsole.SCREEN )
      updateText();
  };
  airconsole.onCustomDeviceStateChange = function(from, data) {
	if (from == AirConsole.SCREEN && data == "Play") {
      state = "playing";
    }
    if (from == AirConsole.SCREEN && data == "Vote") {
      state = "voting";
    }
    if (from == AirConsole.SCREEN && data == "Join") {
      state = "joining";
    }
    if (from == AirConsole.SCREEN && data == "Wait") {
      state = "waiting";
    }
	updateText();
  };
  airconsole.onActivePlayersChange = function (player_number)
  {
    //updateText(player_number);
    updateText();
  };
  airconsole.onReady = function ()
  {
    updateText();
  };
  // wrap in RateLimiter
  airconsole = new RateLimiter(airconsole);
});

function updateText()
{
  if (state == "waiting") {
	$("#please_wait").show();
	$("#left_side").hide();
	$("#right_side").hide();
  } else {
	$("#please_wait").hide();
	$("#left_side").show();
	$("#right_side").show();
	
    if(state == "playing") {
      $("#action1lbl").html("Interact");
      $("#action2").hide();
    }
    if (state == "joining") {
      $("#action1lbl").html("Start game");
      $("#action2").hide();
    }
    if (state == "voting") {
      $("#action1lbl").html("Yes");
      $("#action2lbl").html("No");
      $("#action2").show();
    }
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
}

function requestFocus()
{
  airconsole.message(AirConsole.SCREEN, { focus: 1 });

  $(".focus_button").addClass("active");
  setTimeout(function() {
    $(".focus_button").removeClass("active");
  }, 250);
}

