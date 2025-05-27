const os = require('os');

exports.getLocalTimeNow = () =>{
    const now = new Date();

    // UTC + 9시간 추가
    const kstDate = new Date(now.getTime() + 9 * 60 * 60 * 1000);

    // ISO 포맷으로 수동 조합
    const year = kstDate.getUTCFullYear();
    const month = String(kstDate.getUTCMonth() + 1).padStart(2, '0');
    const day = String(kstDate.getUTCDate()).padStart(2, '0');
    const hours = String(kstDate.getUTCHours()).padStart(2, '0');
    const minutes = String(kstDate.getUTCMinutes()).padStart(2, '0');
    const seconds = String(kstDate.getUTCSeconds()).padStart(2, '0');

    // 최종 문자열
    kstISOString = `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    return kstISOString
}

exports.getLocalIPAddress = () => {
    const interfaces = os.networkInterfaces();
    for (const name of Object.keys(interfaces)) {
        for (const iface of interfaces[name]) {
            if (iface.family === 'IPv4' && !iface.internal) {
                return iface.address;
            }
        }
    }
    return '127.0.0.1'; // fallback
}
