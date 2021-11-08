# OnScreenKeyboardManager
Automatically opens an On-Screen-Keyboard whenever an Input control is selected for Windows 10

# What does it do
This is a small utility that helps you when using an Windows PC with an Touch screen by automatically opening and closing an On-Screen-Keyboard. it listens for Focus events of Text intput Controls, and opens the On-Screen-Keyboard whenever on of these controls is getting focused. 

So basically whenever you select an Text-box, the OSK pops up. Thats it. Nothing more, nothing less.

# Why i created it
I had the requirement to use some good old Win32 applications on an Touch screen PC, running Windows 10. The problem that i quickly faced was, how the users are goning to input text if there is not Keyboard connected. 
I tried to use the windows integrated tools "OSK.exe" and "TabTip.exe", but they never quite worked for my use case, especially because the applications i am running, are exclusive full screen applications, to there is no way the user can open the OSK manually.

Since i could not find an satisfactory solution, i just made one myself.

# Installation
Currently it relies on the "On-Screen-Keyboard" from [FreeVK](URL "https://freevirtualkeyboard.com/"). You have to download it separately and put it in the same directory. Please give credit to them for their most excelent work!

Apart from that, it is just an Exe file, with no dependencies (except standard Windows libraries).
You start the application and it runs in the background until you focus an Text box. Usually you should start it manually or make an Scheduled task when the user logs on.
 
# How it works
Basically it works by registering an window message hook so we can intercept all focus events. Every time the focus changes from one control to another on any application, we get an notification.
When we get an focus notification we check the window class for the control that got the focus, and match it against an list of known Text input class names. if it is an known input control, then we open the OSK.
So ther is already an big problem. We have to match against an known list. So if an control class name is not yet known, it will not get recognized. this is where you come in.

Currenlty we recognize:
* Win32 native Edit controls
* WinForms TextBox controls
* Scintilla

What is NOT supported
* Windows command prompt (aka. cmd). There are some issues that i did not have time to investigate yet.
* Browser. Web pages are absolutly NOT supported!

# Future
* add ability to configure "Known" Class Names. Currently these are only changeable via Source code
* make an UI to be able to configure "Known" Class Names and Keyboard to be used.
* Add support for the windows native "TabTip.exe"
* Add support for more Edit controls.