navigator.vibrate = (navigator.vibrate ||
                         navigator.webkitVibrate ||
                         navigator.mozVibrate ||
                         navigator.msVibrate);

var airconsole;
var state = "waiting";

/**
  * Sets up the communication to the screen.
  */
function init()
{
  airconsole = new AirConsole({"orientation": "landscape"});
  airconsole.onMessage = function (from, data)
  {
    console.log(data);
    if (from == AirConsole.SCREEN && data.vibrate)
    {
      navigator.vibrate(data.vibrate);
      console.log("Vibrating: " + data.vibrate);
    }
    if (from == AirConsole.SCREEN && data == "GameStarts")
    {
      state = "playing";
      console.log("playing");    
    }
    if (from == AirConsole.SCREEN && data == "BeginVote")
    {
      state = "voting";
    }
    if (from == AirConsole.SCREEN && data == "EndVote")
    {
      state = "playing";
    }
    if (from == AirConsole.SCREEN && data.addItem)
    {
      console.log("addItem");
      var elem = document.getElementById("inventory_item_" + data.slot);
      elem.style.backgroundImage = "url('" + data.addItem + ".png')";
      console.log(elem.style.backgroundImage);
      navigator.vibrate(1000);
    }
    if (from == AirConsole.SCREEN && data.removeItem) {
      console.log("removeItem");
      var elem = document.getElementById("inventory_item_" + data.slot);
      elem.style.backgroundImage = "url('btn_item.png')";
      console.log(elem.style.backgroundImage);
    }

    if (from == AirConsole.SCREEN )
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
}
/*
function updateText(player_number)
{
  var div = document.getElementById("action1lbl");
  if (airconsole.getActivePlayerDeviceIds().length == 0) {
    div.innerHTML = "Waiting for more players.";
  } else if (player_number == undefined) {
    div.innerHTML = "This is a 2 player game";
  } else if (player_number == 0) {
    div.innerHTML = "You are the player on the left";
  } else if (player_number == 1) {
    div.innerHTML = "You are the player on the right";
  }  
}*/
function updateText()
{
  var div = document.getElementById("action1lbl");
  var div2 = document.getElementById("action2lbl");
  if(state == "playing")
  {
    div.innerHTML = "Interact";
    div2.innerHTML = "";
    document.getElementById("action2").style.visibility = "hidden";
  }
  if (state == "waiting")
  {
    div.innerHTML = "Start game";
    document.getElementById("action2").style.visibility = "hidden";
  }
  if (state == "voting")
  {
    div.innerHTML = "Yes";
    div2.innerHTML = "No";
    document.getElementById("action2").style.visibility = "visible";
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
	console.log("move");
}
function action1()
{
  console.log("action");
  if(state === "waiting")
  {
    airconsole.message(AirConsole.SCREEN, { start: 1 });
  }
  if (state === "playing")
  {
    airconsole.message(AirConsole.SCREEN, { action: 1 });
  }
  if (state === "voting")
  {
    airconsole.message(AirConsole.SCREEN, { vote: 1 });
  }
}
function action2()
{
  if (state === "playing")
  {
    airconsole.message(AirConsole.SCREEN, { action: 2 });
  }
  if (state === "voting")
  {
    airconsole.message(AirConsole.SCREEN, { vote: 2 });
  }
}

function itemUsed(id)
{
  console.log("item");
  console.log(id);
  airconsole.message(AirConsole.SCREEN, { itemUsed: id });
}

function requestFocus()
{
  console.log("request focus");
  airconsole.message(AirConsole.SCREEN, { focus: 1 });
}

