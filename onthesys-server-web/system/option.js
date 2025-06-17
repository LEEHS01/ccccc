


// Unity WebGL 빌드 폴더 경로 지정
exports.buildPath = 'C:\\Users\\onthesys\\Downloads\\BuildWeb_250612_1412';

// SQL Server 연결 정보
exports.dbConfig = {
    user: 'DBMASTER',
    password: 'admin123!',
    server: '192.168.1.20',       // 예: 'localhost' 또는 '192.168.0.100'
    port: 1933,
    database: 'WEB_DP', // 기본 연결 DB (다른 DB는 3-part name으로 접근)
    options: {
        encrypt: false,          // 내부망이면 false
        trustServerCertificate: true
    }
};

exports.managerPassword = 'Onthesys@12'