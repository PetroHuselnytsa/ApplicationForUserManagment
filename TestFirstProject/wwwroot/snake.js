// === AUTH ===
const API_BASE = '/api';
let currentToken = localStorage.getItem('snakeToken');
let currentUserId = localStorage.getItem('snakeUserId');
let currentUsername = localStorage.getItem('snakeUsername');

async function authFetch(url, options = {}) {
    const headers = { 'Content-Type': 'application/json', ...options.headers };
    if (currentToken) headers['Authorization'] = `Bearer ${currentToken}`;
    const res = await fetch(API_BASE + url, { ...options, headers });
    if (res.status === 401) { logout(); throw new Error('Session expired'); }
    return res;
}

async function handleLogin() {
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;
    const errorEl = document.getElementById('loginError');
    errorEl.classList.add('hidden');

    try {
        const res = await fetch(API_BASE + '/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });
        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.message || 'Login failed');
        }
        const data = await res.json();
        currentToken = data.token;
        currentUserId = data.userId;
        currentUsername = data.username;
        localStorage.setItem('snakeToken', data.token);
        localStorage.setItem('snakeUserId', data.userId);
        localStorage.setItem('snakeUsername', data.username);
        showGameSection();
    } catch (e) {
        errorEl.textContent = e.message;
        errorEl.classList.remove('hidden');
    }
}

function logout() {
    localStorage.removeItem('snakeToken');
    localStorage.removeItem('snakeUserId');
    localStorage.removeItem('snakeUsername');
    currentToken = null;
    document.getElementById('loginSection').classList.remove('hidden');
    document.getElementById('gameSection').classList.add('hidden');
}

function showGameSection() {
    document.getElementById('loginSection').classList.add('hidden');
    document.getElementById('gameSection').classList.remove('hidden');
    document.getElementById('userGreeting').textContent = `Welcome, ${currentUsername}!`;
    document.getElementById('userGreeting').classList.remove('hidden');
    loadPlayerStats();
    loadLeaderboard();
}

// === GAME CONSTANTS ===
const CELL_SIZE = 20;
const COLS = 30;
const ROWS = 20;
const TICK_MS = 150;

const FOOD_POINTS = 10;
const ENEMY_KILL_POINTS = 50;
const SURVIVAL_POINTS_PER_SEC = 1;
const COMBO_WINDOW_MS = 2000;
const MAX_COMBO = 5;
const ENEMY_RESPAWN_DELAY_MS = 3000;

const DIFFICULTY_CONFIG = {
    Easy:   { enemyCount: 1, chaseChance: 0.6 },
    Medium: { enemyCount: 2, chaseChance: 0.75 },
    Hard:   { enemyCount: 3, chaseChance: 0.85 }
};

// === GAME STATE ===
let canvas, ctx;
let snake = [];
let direction = { x: 1, y: 0 };
let nextDirection = { x: 1, y: 0 };
let food = null;
let enemies = [];
let score = 0;
let combo = 1;
let lastFoodTime = 0;
let maxCombo = 0;
let foodEaten = 0;
let enemiesDefeated = 0;
let gameStartTime = 0;
let gameRunning = false;
let gameDifficulty = 'Easy';
let gameLoopInterval = null;

// === GAME LOOP ===
function selectDifficulty(btn) {
    document.querySelectorAll('.diff-btn').forEach(b => b.classList.remove('selected'));
    btn.classList.add('selected');
}

function startGame() {
    gameDifficulty = document.querySelector('.diff-btn.selected')?.dataset.difficulty || 'Easy';
    canvas = document.getElementById('gameCanvas');
    ctx = canvas.getContext('2d');

    // Reset state
    snake = [{ x: 5, y: 10 }, { x: 4, y: 10 }, { x: 3, y: 10 }];
    direction = { x: 1, y: 0 };
    nextDirection = { x: 1, y: 0 };
    score = 0; combo = 1; maxCombo = 0; foodEaten = 0; enemiesDefeated = 0;
    lastFoodTime = 0; enemies = [];
    gameStartTime = Date.now();
    gameRunning = true;

    document.getElementById('gameOverOverlay').classList.add('hidden');
    document.getElementById('setupArea').classList.add('hidden');

    spawnFood();
    spawnInitialEnemies();

    if (gameLoopInterval) clearInterval(gameLoopInterval);
    gameLoopInterval = setInterval(gameTick, TICK_MS);
}

function gameTick() {
    if (!gameRunning) return;

    direction = { ...nextDirection };
    // Move snake head
    const head = { x: snake[0].x + direction.x, y: snake[0].y + direction.y };

    // Wall collision
    if (head.x < 0 || head.x >= COLS || head.y < 0 || head.y >= ROWS) { gameOver(); return; }
    // Self collision
    if (snake.some(s => s.x === head.x && s.y === head.y)) { gameOver(); return; }
    // Enemy head collision (player dies if enemy head is at same position)
    for (const enemy of enemies) {
        if (enemy.alive && enemy.segments[0].x === head.x && enemy.segments[0].y === head.y) {
            gameOver(); return;
        }
    }

    snake.unshift(head);

    // Food check
    if (food && head.x === food.x && head.y === food.y) {
        const now = Date.now();
        if (lastFoodTime && (now - lastFoodTime) < COMBO_WINDOW_MS) {
            combo = Math.min(combo + 1, MAX_COMBO);
        } else {
            combo = 1;
        }
        if (combo > maxCombo) maxCombo = combo;
        lastFoodTime = now;
        score += FOOD_POINTS * combo;
        foodEaten++;
        spawnFood();
    } else {
        snake.pop();
    }

    // Survival points
    score += SURVIVAL_POINTS_PER_SEC * (TICK_MS / 1000);

    // Move enemies & check trapping
    moveEnemies();
    checkEnemyTrapped();
    respawnEnemies();

    // Render
    render();
    updateHud();
}

function render() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Grid background
    ctx.fillStyle = '#f9f9f9';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Food
    if (food) {
        ctx.fillStyle = '#ff4444';
        ctx.beginPath();
        ctx.arc(food.x * CELL_SIZE + CELL_SIZE / 2, food.y * CELL_SIZE + CELL_SIZE / 2, CELL_SIZE / 2 - 2, 0, Math.PI * 2);
        ctx.fill();
    }

    // Player snake
    snake.forEach((seg, i) => {
        ctx.fillStyle = i === 0 ? '#228B22' : '#32cd32';
        ctx.fillRect(seg.x * CELL_SIZE + 1, seg.y * CELL_SIZE + 1, CELL_SIZE - 2, CELL_SIZE - 2);
    });

    // Enemy snakes
    enemies.forEach(enemy => {
        if (!enemy.alive) return;
        enemy.segments.forEach((seg, i) => {
            ctx.fillStyle = i === 0 ? '#8B0000' : '#d9534f';
            ctx.fillRect(seg.x * CELL_SIZE + 1, seg.y * CELL_SIZE + 1, CELL_SIZE - 2, CELL_SIZE - 2);
        });
    });
}

function updateHud() {
    document.getElementById('hudScore').textContent = Math.floor(score);
    document.getElementById('hudCombo').textContent = `x${combo}`;
    document.getElementById('hudCombo').className = combo > 1 ? 'combo-active' : '';
    const elapsed = Math.floor((Date.now() - gameStartTime) / 1000);
    document.getElementById('hudTime').textContent = `${elapsed}s`;
    document.getElementById('hudEnemies').textContent = enemiesDefeated;
}

// === FOOD & ENEMIES ===
function spawnFood() {
    const occupied = new Set();
    snake.forEach(s => occupied.add(`${s.x},${s.y}`));
    enemies.forEach(e => e.segments.forEach(s => occupied.add(`${s.x},${s.y}`)));

    let attempts = 0;
    do {
        food = { x: Math.floor(Math.random() * COLS), y: Math.floor(Math.random() * ROWS) };
        attempts++;
    } while (occupied.has(`${food.x},${food.y}`) && attempts < 100);
}

function spawnInitialEnemies() {
    const config = DIFFICULTY_CONFIG[gameDifficulty];
    const spawnPositions = [
        { x: COLS - 5, y: 3 },
        { x: COLS - 5, y: ROWS - 4 },
        { x: 2, y: 3 }
    ];
    for (let i = 0; i < config.enemyCount; i++) {
        const pos = spawnPositions[i % spawnPositions.length];
        enemies.push({
            segments: [{ x: pos.x, y: pos.y }, { x: pos.x - 1, y: pos.y }, { x: pos.x - 2, y: pos.y }],
            direction: { x: 1, y: 0 },
            alive: true,
            respawnAt: 0
        });
    }
}

function moveEnemies() {
    const config = DIFFICULTY_CONFIG[gameDifficulty];
    enemies.forEach(enemy => {
        if (!enemy.alive) return;
        const head = enemy.segments[0];
        const playerHead = snake[0];

        let newDir;
        if (Math.random() < config.chaseChance) {
            // Greedy chase: reduce Manhattan distance to player head
            const dx = playerHead.x - head.x;
            const dy = playerHead.y - head.y;
            if (Math.abs(dx) > Math.abs(dy)) {
                newDir = { x: dx > 0 ? 1 : -1, y: 0 };
            } else if (Math.abs(dy) > 0) {
                newDir = { x: 0, y: dy > 0 ? 1 : -1 };
            } else {
                newDir = randomDirection();
            }
        } else {
            newDir = randomDirection();
        }

        // Prevent 180-degree reversal
        if (enemy.segments.length > 1) {
            const neck = enemy.segments[1];
            const wouldReverse = (head.x + newDir.x === neck.x && head.y + newDir.y === neck.y);
            if (wouldReverse) newDir = enemy.direction;
        }

        enemy.direction = newDir;
        const newHead = { x: head.x + newDir.x, y: head.y + newDir.y };

        // Wall wrap for enemies
        newHead.x = ((newHead.x % COLS) + COLS) % COLS;
        newHead.y = ((newHead.y % ROWS) + ROWS) % ROWS;

        enemy.segments.unshift(newHead);
        enemy.segments.pop();
    });
}

function randomDirection() {
    const dirs = [{ x: 1, y: 0 }, { x: -1, y: 0 }, { x: 0, y: 1 }, { x: 0, y: -1 }];
    return dirs[Math.floor(Math.random() * dirs.length)];
}

function checkEnemyTrapped() {
    enemies.forEach(enemy => {
        if (!enemy.alive) return;
        const enemyHead = enemy.segments[0];
        // Enemy is trapped if its head collides with player body (not head)
        const hitBody = snake.slice(1).some(s => s.x === enemyHead.x && s.y === enemyHead.y);
        if (hitBody) {
            enemy.alive = false;
            enemy.respawnAt = Date.now() + ENEMY_RESPAWN_DELAY_MS;
            enemiesDefeated++;
            score += ENEMY_KILL_POINTS;
        }
    });
}

function respawnEnemies() {
    const now = Date.now();
    enemies.forEach((enemy, i) => {
        if (!enemy.alive && enemy.respawnAt > 0 && now >= enemy.respawnAt) {
            const corners = [
                { x: COLS - 3, y: 2 }, { x: COLS - 3, y: ROWS - 3 },
                { x: 2, y: 2 }, { x: 2, y: ROWS - 3 }
            ];
            const pos = corners[i % corners.length];
            enemy.segments = [{ x: pos.x, y: pos.y }, { x: pos.x - 1, y: pos.y }, { x: pos.x - 2, y: pos.y }];
            enemy.direction = { x: 1, y: 0 };
            enemy.alive = true;
            enemy.respawnAt = 0;
        }
    });
}

// === KEYBOARD INPUT ===
document.addEventListener('keydown', (e) => {
    if (!gameRunning) return;
    const key = e.key;
    const dirMap = {
        'ArrowUp':    { x: 0, y: -1 }, 'w': { x: 0, y: -1 }, 'W': { x: 0, y: -1 },
        'ArrowDown':  { x: 0, y: 1 },  's': { x: 0, y: 1 },  'S': { x: 0, y: 1 },
        'ArrowLeft':  { x: -1, y: 0 }, 'a': { x: -1, y: 0 }, 'A': { x: -1, y: 0 },
        'ArrowRight': { x: 1, y: 0 },  'd': { x: 1, y: 0 },  'D': { x: 1, y: 0 },
    };
    const newDir = dirMap[key];
    if (!newDir) return;

    // Prevent 180-degree reversal
    if (newDir.x !== -direction.x || newDir.y !== -direction.y) {
        nextDirection = newDir;
    }
    e.preventDefault();
});

// === GAME OVER & API ===
async function gameOver() {
    gameRunning = false;
    if (gameLoopInterval) { clearInterval(gameLoopInterval); gameLoopInterval = null; }

    const durationSeconds = Math.floor((Date.now() - gameStartTime) / 1000);
    const finalScore = Math.floor(score);

    try {
        const res = await authFetch('/snake/scores', {
            method: 'POST',
            body: JSON.stringify({
                score: finalScore,
                difficulty: gameDifficulty,
                durationSeconds,
                enemiesDefeated,
                foodEaten,
                maxCombo
            })
        });
        if (res.ok) {
            const data = await res.json();
            const changeEl = document.getElementById('ratingChangeDisplay');
            const change = data.ratingChange;
            changeEl.textContent = `Rating: ${data.newRating} (${change >= 0 ? '+' : ''}${change})`;
            changeEl.className = `game-over-stats ${change >= 0 ? 'rating-change positive' : 'rating-change negative'}`;
            loadPlayerStats();
            loadLeaderboard();
        }
    } catch (e) {
        console.error('Failed to submit score:', e);
    }

    // Show overlay
    document.getElementById('finalScore').textContent = `Score: ${finalScore}`;
    document.getElementById('finalEnemies').textContent = `Enemies Defeated: ${enemiesDefeated}`;
    document.getElementById('finalCombo').textContent = `Max Combo: x${maxCombo}`;
    document.getElementById('gameOverOverlay').classList.remove('hidden');
}

function resetGame() {
    document.getElementById('gameOverOverlay').classList.add('hidden');
    document.getElementById('setupArea').classList.remove('hidden');
}

// === API: Stats & Leaderboard ===
async function loadPlayerStats() {
    try {
        const res = await authFetch('/snake/stats');
        if (res.ok) {
            const data = await res.json();
            document.getElementById('statRating').textContent = data.rating;
            document.getElementById('statGames').textContent = data.gamesPlayed;
            document.getElementById('statHighScore').textContent = data.highestScore;
        }
    } catch (e) { console.error('Failed to load stats:', e); }
}

let currentPage = 1;
async function loadLeaderboard() {
    const period = document.querySelector('.tab-btn.active')?.dataset.period || 'all';
    try {
        const res = await authFetch(`/snake/leaderboard?period=${period}&page=${currentPage}&pageSize=10`);
        if (res.ok) {
            const data = await res.json();
            const tbody = document.getElementById('leaderboardBody');
            if (data.entries.length === 0) {
                tbody.innerHTML = '<tr><td colspan="4">No scores yet</td></tr>';
            } else {
                tbody.innerHTML = data.entries.map(e => {
                    const isCurrent = e.userId === currentUserId;
                    return `<tr class="${isCurrent ? 'current-user' : ''}">
                        <td>${e.rank}</td>
                        <td>${e.username}</td>
                        <td>${e.score}</td>
                        <td>${e.difficulty}</td>
                    </tr>`;
                }).join('');
            }
            document.getElementById('pageInfo').textContent = `${currentPage} / ${Math.max(1, Math.ceil(data.totalCount / data.pageSize))}`;
            document.getElementById('prevPageBtn').disabled = currentPage <= 1;
            document.getElementById('nextPageBtn').disabled = currentPage * data.pageSize >= data.totalCount;
        }
    } catch (e) { console.error('Failed to load leaderboard:', e); }
}

function changePage(delta) {
    currentPage = Math.max(1, currentPage + delta);
    loadLeaderboard();
}

// === INIT ===
document.addEventListener('DOMContentLoaded', () => {
    if (currentToken) showGameSection();

    // Leaderboard tab handlers
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            currentPage = 1;
            loadLeaderboard();
        });
    });
});
