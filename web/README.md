# Site web BadPig (Angular)

Voir le README à la racine du dépôt. Le site est servi par le BFF (`../bff`) : connexion par cookie, appels `/api` relayés
vers l'API. Il n'y a plus de configuration à lire au démarrage ni de jeton dans le navigateur.

Compilation : `npm ci && npx ng build` (résultat dans `dist/web/browser`, copié dans l'image du BFF par `../bff/Dockerfile`).
