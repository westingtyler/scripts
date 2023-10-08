//=============================================================================
// West_ZoomListener.js
//=============================================================================

/*:
 * @plugindesc Custom Zoom Listener Plugin for RPG Maker MV
 * @author ChatGPT 3.5 Turbo and westingtyler.
 * This is my first script for RPG Maker MV. Possibly it already exists 
 * somewhere else, but what's the fun in that?
 * version 0.9: 2023-10-08.
 *
 * @help This plugin allows you to control the zoom level in your RPG Maker 
 * MV game using the mouse scroll wheel.  
 * 
 * DEPENDENCIES:
 * This script requires that Galv's Screen Zoom script is installed in a 
 * plugin, probably above this one in the plugin manager. Tested with 
 * Galv's Screen Zoom script version 1.2.
 *
 * PLUGIN COMMAND:
 * ZoomListener Update
 * The command updates the zoom level based on mouse scroll wheel input. 
 * Just drop that as a plugin command into a parallel process event on a map.

 * LOWER YOUR CHARACTER:
 * Also, go into Galv's Screen Zoom script in Visual Studio Code,
 * and go to line 105, and change the 12 to 42. It makes my character 
 * appear lower on the screen. See if this does anything for you.
 * Here's the full line 105 under the Galv.ZOOM.target section:
 * 
 * var y = target.screenY() - 12 - scale;
 * to
 * var y = target.screenY() - 42 - scale;
 * 
 * NOTES:
 * You could potentially edit this script to use different buttons
 * for zooming than the scrollwheel. For example T and G. Just paste
 * the script into ChatGPT and ask for help like I did when asking
 * it to make this script.
 * 
 * Remember that while playtesting in RPG Maker MV, you can press F12
 * to bring up the console to see errors and console logs you've added,
 * to test script changes.
 * 
 * TO DO:
 * -See if I can remove the ZoomVariableId parameter, which is maybe unused.
 * -Implement the InvertedZooming bool.
 * 
 * param ZoomVariableId
 * The ID of the variable to store the current zoom level.
 * No reason to really mess with this. Might remove it as a parameter
 * if unneeded.
 * default 1
 * 
 * param InvertedZooming
 * Set to true if you want inverted zooming (scroll up to zoom out).
 * Not implemented yet.
 * default false
 * 
 * param CamFollowsWhileZoomed
 * This would perform the Galv zoom function again each frame but only
 * (while you are zoomed at a non-1 zoom level, and while this is true). 
 * It prevents the character from walking off the screen of small maps
 * after using the Galv zoom function. But it does this by redoing the
 * function each frame, which may be expensive. It also
 * seems to make the main character jitter visually while moving,
 * but this seems to happen only when combined with Galv_PixelMove script, 
 * these scripts fighting for who gets the "late update" or something.
 * If we can figure out how to use some kind of javascript "Late Update"
 * function, this jitter may be resolvable.
 * A smoother looking option than using this bool is to make your maps 
 * so large with borders that the zoomed camera will never hit the 
 * walls of the level preventing your character from walking out of 
 * the zoomed area. But for testing purposes, this bool is nice. Or 
 * Not testing, if you would rather have the hitter than give all 
 * your maps big borders.
 * default false
 *
 * @param ZoomVariableId
 * @desc The ID of the variable to store the current zoom level.
 * @default 1
 * 
 * @param InvertedZooming
 * @desc Set to true if you want inverted zooming (scroll up to zoom out).
 * Not implemented yet.
 * @default false
 * 
 * @param CamFollowsWhileZoomed
 * @desc This would perform the Galv zoom function again each frame
 * while zoomed.
 * @default false
 * 
 */

(function() 
{
    var parameters = PluginManager.parameters('ZoomListener');
    var minZoom = 1;
    var maxZoom = 4;
    var zoomStep = 0.5;
    var currentZoom = 1;
    var scrollDirection = TouchInput.wheelY;
    var invertedZooming = parameters['InvertedZooming'] === 'true';//not yet implemented
    var camFollowsWhileZoomed = PluginManager.parameters('West_ZoomListener')['CamFollowsWhileZoomed'];

    // Event command handling
    var _Game_Interpreter_pluginCommand = Game_Interpreter.prototype.pluginCommand;
    Game_Interpreter.prototype.pluginCommand = function(command, args) 
    {
        _Game_Interpreter_pluginCommand.call(this, command, args);

        if (command === 'ZoomListener' && args[0] === 'Update') 
        {
            updateZoomLevel();
        }
    };

    function clamp(value, min, max) {
        return Math.min(Math.max(value, min), max);
    }

    function updateZoomLevel(scrollDirection) 
    {
        if (TouchInput.wheelY < 0) {
            currentZoom += zoomStep;
            currentZoom = clamp(currentZoom, minZoom, maxZoom);
            console.log ("scrolling up, zoom in",TouchInput.wheelY, currentZoom);
            updateGameZoom(currentZoom);
        } else if (TouchInput.wheelY > 0) {
            currentZoom -= zoomStep;
            currentZoom = clamp(currentZoom, minZoom, maxZoom);
            console.log ("scrolling up, zoom in",TouchInput.wheelY, currentZoom);
            updateGameZoom(currentZoom);
        }

        if (camFollowsWhileZoomed == 'true') 
        {
            if (currentZoom != 1)
            {
                eval(Galv.ZOOM.target(0, currentZoom, 1));
            }
        }
    }

    function updateGameZoom(currentZoom) 
    {
        eval(Galv.ZOOM.target(0, currentZoom, 1));
    }

})();
