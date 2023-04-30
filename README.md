# Unity-2DPlayerMotor
PlayerMontir script for 2D player Character in Unity

A current project to make the best possible 2D CharacterController in Unity. <br />
My main gaol is to make it user friendly and highly adjustable for different use cases. <br />
And also to write clean code.

---
# Getting Started
In order to get started you need to make a new Unity Project. Once it lauched you`ll create a new Player Object this can be a simple square or a Sprite (depending on what you want). At this point I like to give this object the "Player" Tag, however this is not necessary. <br /> Then you want to create a new Ground Plane (for me this is also just a sqaure). Now asign a BoxCollider2D to this object and add a layer named "Ground".<br /> 
The next thing to do would be to download or copy the code you can find as "PlayerMotor". <br />
To your Player GameObject add the following components with changes shown in the Settings.txt file. <br />

- Rigidbody2D
- BoxCollider2D
- (TrailRenderer)
- PlayerMotor c# script
<br />

Once you are finsihed playing a bit with the values in the Controler script you should have something that looks like that: <br />
![alt text](settings_for_script.png)

<br />

**If you are intresed in more details on the code you might want to check out the [CodeInfo.md](CodeInfo.md) file.**

Asign everything correctly and you`ll be good to go :)


**As im still working on this pice of code there will be updates/news so you might want to check this Github page from time to timne in order to be uptodate with your code. <br /> Fell free to use this code in your project (but giving credit is always nice). I hope you have fun with this PlayerController**
