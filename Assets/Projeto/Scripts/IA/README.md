# Neuroevolução - Unity 6

Scripts para o laboratório da Aula 05. O controlador é uma rede neural `3-4-2` cujos 26 pesos e biases são evoluídos por um algoritmo genético.

## Arquivos

- `Genome.cs`: vetor de pesos, crossover e mutação.
- `NeuralNetwork.cs`: inferência da rede neural, sem alocações durante o episódio.
- `EvolvingShip.cs`: Raycasts, movimento, fitness individual e término de episódio.
- `EvolutionManager.cs`: população, elitismo, torneio e troca de gerações.

## Configuração da cena

1. Crie uma cena 3D com um plano, paredes e obstáculos com `Collider`. Crie a layer `Obstacle` e aplique-a a todas as paredes e obstáculos.
2. Crie um objeto `Goal` com `Collider` marcado como `Is Trigger`. O `Transform` desse objeto será o campo **Goal** do manager.
3. Crie um objeto vazio `SpawnPoint` posicionado no início da pista.
4. Crie o prefab `Ship`: um objeto com `Rigidbody`, `Collider` e `EvolvingShip`. Desative `Use Gravity` no Rigidbody. Em **Obstacle Mask**, selecione somente `Obstacle`.
5. Crie um objeto vazio `EvolutionManager`, anexe `EvolutionManager.cs` e associe o prefab, `SpawnPoint` e `Goal`.
6. No `Project Settings > Physics`, desative colisão entre a layer usada pelas naves e ela mesma. Assim, os indivíduos não atrapalham a avaliação uns dos outros.
7. Execute a cena. O Console registra o melhor e o fitness médio a cada geração.

## Fitness padrão

`progresso ate o alvo * 10 + bonus ao chegar - 50 por colisao - 1 por timeout`

O progresso é recalculado continuamente. Assim, uma nave parada não recebe fitness alto apenas por sobreviver.

## Experimentos sugeridos

- Comece com `populationSize = 30`, `mutationRate = 0.08` e `mutationStrength = 0.25`.
- Varie uma configuração por vez e anote melhor fitness, fitness médio e geração em que o comportamento surgiu.
- Mude o `randomSeed` para repetir ou comparar execuções.
- Para validar generalização, mova o alvo depois de encontrar um bom genoma e rode novamente.
