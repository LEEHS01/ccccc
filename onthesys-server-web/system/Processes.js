const local = require('../system/localization')
const sql = require('mssql');
const option = require('../system/option.js');

//프로세스 용 쿼리문
async function UpdateTestMeasureRecent(boardId, sensorId) {
    try {
        await sql.connect(option.dbConfig);

        seed = boardId * 1241 + sensorId * 21414512;
        time = new Date() / 1000/200;

        value = (Math.sin(seed + time) + Math.cos((seed + time) * 1.41) + 2 * Math.sin((seed + time) / 1.41) +4)/8;
        value *= 150
        value += (Math.random()-0.5)*40;
        await sql.query(`
            UPDATE measure_recent
            SET  measured_value = ${value }, measured_time= '${local.getLocalTimeNow()}'
            WHERE board_id = ${boardId} AND sensor_id = ${sensorId}
        `);

    } catch (err) {
        console.error('❌ SQL Error:', err);
    }
}

async function UpdateTestMeasureLogs() {
    try {
        await sql.connect(option.dbConfig);

        const result = await sql.query(`
            SELECT board_id, sensor_id, measured_time, measured_value
            FROM measure_recent
        `);

        const now = new Date();
        //now.setSeconds(0, 0);

        for (const row of result.recordset) {
            const measuredDate = new Date(row.measured_time);
            //measuredDate.setSeconds(0, 0);

            sql.query(`
                INSERT INTO measure_log (board_id, sensor_id, measured_time, measured_value)
                VALUES (${row.board_id}, ${row.sensor_id}, '${measuredDate.toISOString()}', ${row.measured_value})
            `);

        }
    } catch (err) {
        console.error('❌ SQL Error:', err);
    }

}

async function UpdateSensorStatus() {
    try {
        await sql.connect(option.dbConfig);

        // 1️⃣ 모든 센서 데이터 가져오기
        const { recordset: sensors } = await sql.query(`
            SELECT 
                s.board_id, 
                s.sensor_id, 
                s.threshold_critical,
                s.threshold_serious, 
                s.threshold_warning,
                m.measured_value
            FROM sensor s
            JOIN measure_recent m
            ON s.board_id = 1 AND s.sensor_id = m.sensor_id
        `);

        // 2️⃣ 센서 상태 판별 및 업데이트 처리
        for (const sensor of sensors) {
            const { board_id, sensor_id, threshold_critical, threshold_serious, threshold_warning, measured_value } = sensor;

            // 상태 판정
            // let currentStatus;
            // if (measured_value >= threshold_critical) {
            //     currentStatus = 'Critical';
            // } else if (measured_value >= threshold_serious) {
            //     currentStatus = 'Serious';
            // } else if (measured_value >= threshold_warning) {
            //     currentStatus = 'Warning';
            // } else {
            //     currentStatus = 'Normal';
            // }

            console.log(`🌐 Sensor [${board_id}, ${sensor_id}] 상태: ${currentStatus}`);

            // 3️⃣ alarm_log에 활성화된 알람 중 상태가 다른 항목 찾기
            const { recordset: activeAlarms } = await sql.query(`
                SELECT alarm_id, alarm_level
                FROM alarm_log
                WHERE sensor_id = ${sensor_id} 
                  AND solved_time IS NULL
            `);
            if (activeAlarms.length === 0 && currentStatus !== 'Normal') {
                // 🔥 활성화된 알람이 없을 경우 새로운 로그 추가
                // await sql.query(`
                //     INSERT INTO alarm_log (board_id, sensor_id, alarm_level, occured_time, solved_time)
                //     VALUES (${board_id}, ${sensor_id}, '${currentStatus}', GETDATE(), NULL)
                // `);
                console.log(`🆕 New alarm created for [${board_id}, ${sensor_id}] with status ${currentStatus}`);
            }

            for (const alarm of activeAlarms) {
                if (alarm.alarm_level !== currentStatus) {
                    if (currentStatus === 'Normal') {
                        // await sql.query(`
                        //     UPDATE alarm_log
                        //     SET solved_time = GETDATE()
                        //     WHERE alarm_id = ${alarm.alarm_id}
                        // `);
                        console.log(`✅ Alarm ${alarm.alarm_id} solved at ${new Date().toISOString()}`);
                    } else {
                        // await sql.query(`
                        //     UPDATE alarm_log
                        //     SET alarm_level = '${currentStatus}',
                        //         occured_time = GETDATE()
                        //     WHERE alarm_id = ${alarm.alarm_id}
                        // `);
                        console.log(`✅ Alarm ${alarm.alarm_id} updated to ${currentStatus}`);
                    }
                }
            }
        }
    } catch (err) {
        console.error('❌ SQL Error:', err.message);
    }
}

exports.StartProcess = ()=>
{
    //프로세스
    // setInterval(() => {
    //     UpdateTestMeasureLogs();

    //     console.log('✅', local.getLocalTimeNow(), ' : ',
    //         'log has been updated.');
    // }, 11 * 1000)

    // setInterval(() => {
    //     UpdateTestMeasureRecent(1, 1);
    //     UpdateTestMeasureRecent(1, 2);
    //     UpdateTestMeasureRecent(1, 3);
    //     UpdateTestMeasureRecent(2, 1);
    //     UpdateTestMeasureRecent(2, 2);
    //     UpdateTestMeasureRecent(2, 3);

    //     console.log('✅', local.getLocalTimeNow(), ' : ',
    //         'recent values has been updated.');
    // }, 10 * 1000);

    // setInterval(() => {
    //     UpdateSensorStatus();
    // }, 10 * 1000);
}