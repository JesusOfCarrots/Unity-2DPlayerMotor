The Move Speed determains, well, the move speed. It is later multiplied with '10' so keep it low. <br />

The Jump Force is later influenced by other variables, just keep it around 10. The Can Jump bool is mostly for debug reasons.  <br />

Create an Empty Game Object, position it at the Players feet and asign it.  <br />
Also select the Ground Layer and set the Radius.  <br /> 

Now for the Wall Jumping. The Wall Jump Time, Wall Slide Speed, Wall Distance and X Force all do what their names say(whereby the last one doesn't work). Just keep them around the values you can see in the Picture.  <br />

The (once again) Jump section influences the Jump, by aplying aditional down force the longer the Player is falling.  <br /> 
The Fall Multiplier does what I just explained while the Low Jump Multiplier makes the Jump heigher the longer the Player is pressing the button.  <br />

Damping. Damping is the 'acceleration'. The first one is the Basic acceleration, the second one is the acceleration when Stopping and last but not least the third one is the Dampign when Turning.  <br />

Dashing: The Can Dash bool is just for debugging; ignore it.  <br /> 
Dash Power is self-explanatory I think.  <br />
Dashing Time is the amount of Time the Player ist dashing.  <br />
Dashing Cooldown is the Time between Dashes.  <br />

The Tr is the Trail Renderer which is shown while dashing.  <br />
