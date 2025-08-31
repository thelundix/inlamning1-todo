const API_BASE = localStorage.getItem('apiBase') || 'https://localhost:7088'; // ändra vid behov

const els = {
  auth: document.getElementById('auth'),
  app: document.getElementById('app'),
  loginForm: document.getElementById('login-form'),
  username: document.getElementById('username'),
  password: document.getElementById('password'),
  loginError: document.getElementById('login-error'),
  who: document.getElementById('who'),
  roleTag: document.getElementById('roleTag'),
  logout: document.getElementById('logout'),
  todoForm: document.getElementById('todo-form'),
  todoTitle: document.getElementById('todo-title'),
  todoList: document.getElementById('todo-list'),
  admin: document.getElementById('admin'),
  addUserForm: document.getElementById('add-user-form'),
  newUsername: document.getElementById('new-username'),
  newPassword: document.getElementById('new-password'),
  newRole: document.getElementById('new-role'),
  addUserMsg: document.getElementById('add-user-msg'),
};

// Säker token-hantering: sessionStorage (per flik) + minnescache
let memToken = null;
const tokenKey = 'jwt';

function setToken(tok){
  memToken = tok;
  sessionStorage.setItem(tokenKey, tok);
}
function getToken(){
  return memToken || sessionStorage.getItem(tokenKey);
}
function clearToken(){
  memToken = null;
  sessionStorage.removeItem(tokenKey);
}

function parseJwt(token){
  try {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload.replace(/-/g,'+').replace(/_/g,'/')));
  } catch { return null; }
}

async function api(path, options={}){
  const token = getToken();
  const headers = Object.assign({ 'Content-Type':'application/json' }, options.headers || {});
  if(token) headers['Authorization'] = `Bearer ${token}`;
  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });
  if(res.status === 401){
    // token död/ogiltig -> logga ut
    logout();
    throw new Error('Unauthorized');
  }
  return res;
}

async function login(e){
  e.preventDefault();
  els.loginError.hidden = true;
  const res = await fetch(`${API_BASE}/api/auth/login`,{
    method:'POST',
    headers:{'Content-Type':'application/json'},
    body: JSON.stringify({ username: els.username.value.trim(), password: els.password.value })
  });
  if(!res.ok){ els.loginError.hidden = false; return; }
  const data = await res.json();
  setToken(data.token);
  enterApp(data.username, data.role, data.expiresAtUtc);
}

function enterApp(username, role, expUtc){
  els.auth.hidden = true;
  els.app.hidden = false;
  els.who.textContent = username;
  els.roleTag.textContent = role;
  els.admin.hidden = role !== 'Admin';
  loadTodos();
}

function logout(){
  clearToken();
  els.app.hidden = true;
  els.auth.hidden = false;

  // Rensa inloggningsformulär direkt
  if (els.loginForm) els.loginForm.reset();
  els.username.value = '';
  els.password.value = '';
  els.loginError.hidden = true;
  els.username.focus();
}

async function loadTodos(){
  const res = await api('/api/todos');
  const list = await res.json();
  renderTodos(list);
}

function renderTodos(items){
  els.todoList.innerHTML = '';
  for(const t of items){
    const li = document.createElement('li');
    const left = document.createElement('div');
    const title = document.createElement('span');
    title.textContent = t.title;
    if(t.isDone) title.classList.add('done');

    const toggle = document.createElement('input');
    toggle.type = 'checkbox';
    toggle.checked = t.isDone;
    toggle.addEventListener('change', async ()=>{
      await api(`/api/todos/${t.id}`, {
        method: 'PUT',
        body: JSON.stringify({ title: t.title, isDone: toggle.checked })
      });
      loadTodos();
    });

    left.append(toggle, title);

    const del = document.createElement('button');
    del.textContent = 'Radera';
    del.addEventListener('click', async ()=>{
      await api(`/api/todos/${t.id}`, { method: 'DELETE' });
      li.remove();
    });

    li.append(left, del);
    els.todoList.append(li);
  }
}

async function addTodo(e){
  e.preventDefault();
  const title = els.todoTitle.value.trim();
  if(!title) return;
  await api('/api/todos', { method:'POST', body: JSON.stringify({ title }) });
  els.todoTitle.value = '';
  loadTodos();
}

async function addUser(e){
  e.preventDefault();
  els.addUserMsg.textContent = '';
  const payload = {
    username: els.newUsername.value.trim(),
    password: els.newPassword.value,
    role: els.newRole.value
  };
  const res = await api('/api/admin/users', { method:'POST', body: JSON.stringify(payload) });
  if(res.ok){
    els.addUserMsg.textContent = 'Användare skapad!';
    els.newUsername.value = els.newPassword.value = '';
  } else {
    els.addUserMsg.textContent = 'Kunde inte skapa användare (finns användarnamnet redan?).';
  }
}

// Event wiring
els.loginForm.addEventListener('submit', login);
els.logout.addEventListener('click', logout);
els.todoForm.addEventListener('submit', addTodo);
els.addUserForm.addEventListener('submit', addUser);

// Autologin om token finns och inte utgången
(function init(){
  const tok = getToken();
  if(!tok) return;
  const payload = parseJwt(tok);
  if(!payload || (payload.exp * 1000) < Date.now()) { clearToken(); return; }
  enterApp(payload.name, payload.role, payload.exp);
})();
