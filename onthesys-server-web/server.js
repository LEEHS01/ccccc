// server.js
const express = require('express');
const app = express();
const processes = require('./system/Processes')
const local = require('./system/localization')
const mainRoutes = require('./routes/mainRoutes');
const dbRoutes = require('./routes/dbRoutes');
const authRoutes = require('./routes/authRoutes');

const bcrypt = require('bcrypt');

app.use(express.json());
app.use('/',mainRoutes)
app.use('/query',dbRoutes)
app.use('/auth',authRoutes)


processes.StartProcess();

const PORT = process.env.PORT || 8080;
const IP = local.getLocalIPAddress();
app.listen(PORT, IP, () => {
    console.log(`✅ Unity WebGL 서버 실행 중: \x1b]8;;http://${IP}:${PORT}\x1b\\🌐 http://${IP}:${PORT}\x1b]8;;\x1b\\`);
});

