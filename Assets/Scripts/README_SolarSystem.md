# Système Solaire Unity 3D - Guide d'utilisation

## Vue d'ensemble

Ce système de simulation du système solaire pour Unity 3D implémente :
- Extraction et traitement des données planétaires depuis un fichier CSV
- Calculs de trajectoires kepleriennes précises
- Intégration numérique performante (méthode de Newton-Raphson)
- Rotation des planètes sur elles-mêmes
- Tracé des orbites
- Système de caméra existant (fixe et libre)

## Scripts principaux

### 1. PlanetData.cs
Structure de données pour les informations planétaires.
- Contient toutes les données orbitales et physiques
- Calcule les propriétés dérivées (période orbitale, mouvement moyen)

### 2. PlanetDataLoader.cs
Charge et parse les données depuis le fichier CSV.
- Assigne le fichier PlanetsData.csv dans l'inspector
- Parse les données et crée les objets PlanetData

### 3. KeplerianOrbit.cs
Implémente les calculs de mécanique orbitale.
- Résout l'équation de Kepler avec Newton-Raphson
- Calcule les positions et vitesses orbitales
- Transforme les coordonnées orbitales en espace 3D

### 4. PlanetControl.cs
Contrôle individuel de chaque planète.
- Gère la position et la rotation
- Applique les facteurs d'échelle
- Met à jour l'orbite en temps réel

### 5. SolarSystemSimulator.cs
Gestionnaire principal de la simulation.
- Crée et configure toutes les planètes
- Gère les paramètres globaux
- Interface de contrôle

### 6. OrbitDrawer.cs
Tracé des orbites des planètes.
- Dessine les trajectoires orbitales
- Utilise LineRenderer pour la visualisation

### 7. OrbitManager.cs
Gestionnaire des orbites.
- Crée automatiquement les OrbitDrawer pour chaque planète
- Contrôle la visibilité des orbites

### 8. SolarSystemSetup.cs
Script de configuration automatique.
- Configure tous les composants nécessaires
- Interface de configuration dans l'inspector

### 9. SolarSystemUI.cs
Interface utilisateur.
- Contrôles de simulation
- Aide clavier

### 10. PhysicsConstants.cs
Constantes physiques et fonctions utilitaires.

## Configuration rapide

### Étape 1 : Préparation
1. Créez un GameObject vide nommé "SolarSystemManager"
2. Ajoutez le script `SolarSystemSetup`
3. Assignez le fichier `PlanetsData.csv` au champ `dataLoader.csvFile`
4. Assignez les prefabs de planètes dans `planetPrefabs`

### Étape 2 : Configuration des échelles
- **Distance Scale** : 1e-9 (pour voir toutes les planètes)
- **Size Scale** : 1e-6 (pour des tailles visibles)
- **Time Scale** : 1.0 (vitesse normale)

### Étape 3 : Lancement
1. Cliquez sur "Setup Solar System" dans l'inspector
2. La simulation se lance automatiquement

## Contrôles

### Clavier
- **Tab** : Basculer l'interface utilisateur
- **R** : Réinitialiser la simulation
- **O** : Basculer l'affichage des orbites
- **T** : Basculer la rotation des planètes
- **+/-** : Augmenter/diminuer la vitesse
- **Flèches** : Changer de caméra (système existant)

### Souris
- **Clic droit + glisser** : Rotation caméra
- **Molette** : Zoom
- **Z/Q/S/D** : Déplacement caméra
- **R/F** : Monter/descendre

## Paramètres d'échelle

### Distance Scale (1e-9 recommandé)
- Contrôle la distance entre les planètes et le soleil
- Plus petit = planètes plus proches
- Plus grand = planètes plus éloignées

### Size Scale (1e-6 recommandé)
- Contrôle la taille des planètes
- Plus petit = planètes plus petites
- Plus grand = planètes plus grandes

### Time Scale (1.0 = temps réel)
- Contrôle la vitesse de la simulation
- 0.1 = 10x plus lent
- 10.0 = 10x plus rapide

## Personnalisation

### Ajouter une nouvelle planète
1. Ajoutez les données dans `PlanetsData.csv`
2. Créez un prefab pour la planète
3. Ajoutez le prefab à `planetPrefabs` dans `SolarSystemSetup`

### Modifier les orbites
- Les couleurs des orbites sont dans `OrbitManager.orbitColors`
- La largeur des lignes est dans `orbitLineWidth`
- Le nombre de segments est dans `orbitSegments`

### Ajuster les performances
- Réduisez `orbitSegments` pour des orbites moins précises mais plus rapides
- Désactivez `updateOrbit` dans `OrbitDrawer` si les orbites sont statiques

## Dépannage

### Problèmes courants
1. **Planètes invisibles** : Vérifiez les échelles de distance et de taille
2. **Orbites déformées** : Vérifiez que `distanceScale` est cohérent
3. **Performance lente** : Réduisez le nombre de segments d'orbite
4. **Données non chargées** : Vérifiez que le fichier CSV est assigné

### Debug
- Activez `showDebugInfo` dans `SolarSystemSimulator`
- Utilisez `SolarSystemTest` pour tester les composants
- Vérifiez la console pour les messages d'erreur

## Structure des données CSV

Le fichier `PlanetsData.csv` doit contenir :
- Nom de la planète
- Masse (kg)
- Diamètre (m)
- Demi-grand axe (m)
- Excentricité
- Inclinaison (degrés)
- Longitude du nœud ascendant (degrés)
- Argument du périhélie (degrés)
- Anomalie moyenne à l'époque (degrés)
- Époque
- Facteurs d'échelle
- Période de rotation (heures)
- Multiplicateur de vitesse de rotation
- Obliquité de rotation (degrés)

## Notes techniques

- Utilise la méthode de Newton-Raphson pour résoudre l'équation de Kepler
- Précision double pour les calculs orbitaux
- Intégration en temps réel avec pas de temps variable
- Support des orbites elliptiques avec tous les éléments orbitaux
- Rotation réaliste basée sur les périodes de rotation réelles
