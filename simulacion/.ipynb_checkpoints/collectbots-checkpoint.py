#importar librerias necesarias
from mesa import Agent, Model 

#necesitamos que al inicio esten todos los robots en el mismo punto
from mesa.space import MultiGrid
from mesa.space import SingleGrid

#todos los agentes se activen ''al mismo tiempo''.
from mesa.time import RandomActivation

#obtener información de cada paso de la simulación.
from mesa.datacollection import DataCollector

%matplotlib inline
import matplotlib
import matplotlib.pyplot as plt
import matplotlib.animation as animation
plt.rcParams["animation.html"] = "jshtml"
matplotlib.rcParams['animation.embed_limit'] = 2**128

import numpy as np
import pandas as pd
import time
import datetime

height = 0
width = 0
puntoGeneracion = (0, 0)
def leerTxt():
  global height, width, puntoGeneracion
  archivo = "mapaTest.txt"
  matriz = []

  with open(archivo, 'r') as f:
      lineas = f.readlines()

  # Obtener las dimensiones de la matriz
  dimensiones = lineas[0].split()
  filas = int(dimensiones[0])
  columnas = int(dimensiones[1])
  height = filas
  width = columnas
    
  # Construir la matriz
  for linea in lineas[1:]:
      fila = linea.split()
      matriz.append(fila)
  
  for i in range(filas):
      for j in range(columnas):
          if matriz[i][j] == 'S':
              puntoGeneracion = (i, j)

  # Retorna la matriz
  return matriz

mapaInicial = leerTxt()
mapaActual = []

for i in range(len(mapaInicial)):
    mapaActual.append([])
    for j in range(len(mapaInicial[i])):
        if mapaInicial[i][j] == 'P' or mapaInicial[i][j] == 'S':
            mapaActual[i].append(mapaInicial[i][j]);
        else:
            mapaActual[i].append(0);

class RobotExplorador(Agent):
    def __init__(self, id, model, capacidad=5):
        super().__init__(id, model)
        self.capacidad = capacidad
        
    def step(self):
        possible_moves = self.model.grid.get_neighborhood(
            self.pos,
            moore=True,
            include_center=False
        )
                
        x, y = self.pos
                
        if mapaInicial[x][y] != 'P':
            mapaInicial[x][y] = 'V'
        
        empty_cells = [cell for cell in possible_moves if self.model.grid.is_cell_empty(cell) and mapaInicial[cell[0]][cell[1]] != 'X' and mapaInicial[cell[0]][cell[1]] != 'P']
        
        if empty_cells:  # Si hay celdas vacías alrededor
            new_position = self.random.choice(empty_cells)
            self.model.grid.move_agent(self, new_position)

def get_grid(model):
    grid = np.zeros( (model.grid.width, model.grid.height) )    
    for (content, (x, y)) in model.grid.coord_iter():
        if content:
            grid[x][y] = 1
        else:
            if mapaInicial[x][y] == 'X':
                grid[x][y] = 2
            elif mapaInicial[x][y] == 'P':
                grid[x][y] = 3
            elif mapaInicial[x][y] == 'V':
                grid[x][y] = 4
    print(grid)
    return grid

class EspacioModel(Model):
    def __init__(self, width, height):
        self.grid = MultiGrid(width, height, torus = False)
        self.schedule = RandomActivation(self)
        self.datacollector = DataCollector(model_reporters = {"Grid": get_grid})

        id = 0
        num_agents = 5        
        while self.grid.exists_empty_cells():
            agent = RobotExplorador(id, self)            
            self.grid.place_agent(agent, puntoGeneracion)
            self.schedule.add(agent)
            id += 1
            if id >= num_agents:
                break
            
    def step(self):
        self.datacollector.collect(self)
        self.schedule.step()

MAX_ITERATIONS = 100
print(height, width)
model = EspacioModel(height, width)
for i in range(MAX_ITERATIONS):
    model.step()

all_grid = model.datacollector.get_model_vars_dataframe()
color_map = {
    0: 'white',  
    1: 'blue',   
    2: 'red',
    3: 'green',
    4: 'yellow'
}
fig, axis = plt.subplots(figsize=(5,5))
axis.set_xticks([])
axis.set_yticks([])
patch = plt.imshow(all_grid.iloc[0][0], cmap=plt.cm.binary)
def animate(i):
  patch.set_data(all_grid.iloc[i][0])

anim = animation.FuncAnimation(fig, animate, frames=MAX_ITERATIONS)
anim