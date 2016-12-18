navigator.vibrate = (navigator.vibrate ||
                         navigator.webkitVibrate ||
                         navigator.mozVibrate ||
                         navigator.msVibrate);

    var airconsole;
    /**
     * Sets up the communication to the screen.
     */
    function init() {
      airconsole = new AirConsole({"orientation": "landscape"});
      airconsole.onMessage = function (from, data)
      {
        if (from == AirConsole.SCREEN && data.vibrate) {
          navigator.vibrate(data.vibrate);
          console.log("Vibrating: " + data.vibrate);
        }
      }
      airconsole.onActivePlayersChange = function(player_number) {
        updateText(player_number);
      }
      airconsole.onReady = function() {
        updateText();
      }
    }
/*
	function updateText(player_number) {
	  var div = document.getElementById("player_id");
      if (airconsole.getActivePlayerDeviceIds().length == 0) {
        div.innerHTML = "Waiting for more players.";
      } else if (player_number == undefined) {
        div.innerHTML = "This is a 2 player game";
      } else if (player_number == 0) {
        div.innerHTML = "You are the player on the left";
      } else if (player_number == 1) {
        div.innerHTML = "You are the player on the right";
      };
	}*/

    /**
     * Tells the screen to move the paddle of this player.
     * @param dir
     */
	  function move(dir)
	  {
      //airconsole.message(AirConsole.SCREEN, { move: amount })
	    airconsole.message(AirConsole.SCREEN, { direction: dir })
	    console.log("move");
    }
