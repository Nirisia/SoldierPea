# SoldierPea

SoldierPea is a school project with the purpose of making the AI of an RTS game in Unity 2020.3.
For this we were to use a template given to use, improve and implement new functionnality on.

___

## Table Of Contents

- [SoldierPea](#soldierpea)
    - [Table Of Contents](#table-of-contents)
    - [Authors](#authors)

## Authors

It was made by:

- Amélie Pichard, who implemented the Behaviour Tree (Decision)
- Tristan Francillonne, who implemented the Tactician (Planification)
- Quentin Bleuse, who implemented the squad movement (Flocking)

## Techs

The project was developped under Unity 2020.3.33f1. Any version of Unity.2020.3 should be able to run the project.

We used a template provided by us that included:
 - player control
 - basic gameflow
 - basic UI

## How to run
We have no build so you can play on the viewport of Unity.

## How to play

Left Click - On your base (blue building) to create units 
A - To select all your units
Right Click - On the map to move your selected units
ZQSD or wheel Click - To move the camera

## Additional Notes

- there is still some bug in the planning AI, such as squads stopping.
- some things, such as Make Unit Task, are hardcoded due to the static number of units that we have and the lack of time to do something more modular.

Link : 
- https://medium.com/geekculture/how-to-create-a-simple-behaviour-tree-in-unity-c-3964c84c060e
- https://www.researchgate.net/publication/224491324_Intelligent_Moving_of_Groups_in_Real-Time_Strategy_Games
- https://sandruski.github.io/RTS-Group-Movement/
- https://medium.com/fragmentblog/simulating-flocking-with-the-boids-algorithm-92aef51b9e00
