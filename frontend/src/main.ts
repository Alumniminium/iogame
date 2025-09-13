import { ClientGame } from './game/ClientGame';

// Get DOM elements
const canvas = document.getElementById('gameCanvas') as HTMLCanvasElement;
const loginForm = document.getElementById('loginForm') as HTMLFormElement;
const nameInput = document.getElementById('nameInput') as HTMLInputElement;
const chatInput = document.getElementById('chatInput') as HTMLInputElement;

// Game instance
let game: ClientGame | null = null;

// Load saved username
const savedUsername = localStorage.getItem('playerName');
if (savedUsername && nameInput) {
  nameInput.value = savedUsername;
}

// Auto-login if we have a saved username
if (savedUsername) {
  setTimeout(() => {
    startGame(savedUsername);
  }, 100);
}

// Handle login
if (loginForm) {
  loginForm.addEventListener('submit', (e) => {
    e.preventDefault();

    const playerName = nameInput?.value || 'Player';

    // Save username to localStorage
    localStorage.setItem('playerName', playerName);

    // Start game
    startGame(playerName);
  });
}

// Handle chat
if (chatInput) {
  chatInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && chatInput.value.trim()) {
      if (game) {
        game.sendChat(chatInput.value);
        chatInput.value = '';
      }
    }
  });
}

function startGame(playerName: string) {
  if (!canvas) {
    console.error('Canvas element not found');
    return;
  }

  // Hide login form and show canvas
  if (loginForm) {
    loginForm.style.display = 'none';
  }
  if (canvas) {
    canvas.style.display = 'block';
  }

  // Create and start game
  game = new ClientGame({
    canvas,
    playerName,
    serverUrl: getServerUrl()
  });
  
  // Make game accessible globally for debugging
  (window as any).game = game;
}

function getServerUrl(): string {
  const protocol = window.location.protocol === 'https:' ? 'wss' : 'ws';
  const host = window.location.hostname || 'localhost';
  const port = '5000'; // Server runs on port 5000
  return `${protocol}://${host}:${port}/ws`;
}

// Handle window resize
window.addEventListener('resize', () => {
  if (canvas) {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
  }
});

// Initial canvas setup
if (canvas) {
  canvas.width = window.innerWidth;
  canvas.height = window.innerHeight;
}

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
  if (game) {
    game.disconnect();
  }
});