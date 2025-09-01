const bcrypt = require('bcrypt');
const crypto = require('crypto');
const sql = require('mssql');
const option = require('../system/option');
tokens = {}

exports.certification = async (req, res) => {
  const { item } = req.body;
  console.log("password : ", item);

  try {
    // DB 연결
    const pool = await sql.connect(option.dbConfig);
    
    // DB에서 최신 비밀번호 가져오기
    const result = await pool.request()
      .query('SELECT TOP 1 password FROM admin_password ORDER BY updated_at DESC');
    
    if (result.recordset.length === 0) {
      return res.json({
        is_succeed: false,
        message: '비밀번호 정보를 찾을 수 없습니다.',
        auth_code: ''
      });
    }
    
    const dbPassword = result.recordset[0].password;
    console.log("DB password : ", dbPassword);
    
    // 비밀번호 비교
    if (item !== dbPassword) {
      return res.json({
        is_succeed: false,
        message: '비밀번호가 일치하지 않습니다.',
        auth_code: ''
      });
    }

    // 인증 성공 → 토큰 생성
    const rawToken = crypto.randomBytes(32).toString('hex');
    const expiresAt = Date.now() + 30 * 60 * 1000; // 30분

    tokens[rawToken] = expiresAt;

    res.json({
      is_succeed: true,
      message: '인증에 성공했습니다.',
      auth_code: rawToken
    });

  } catch (err) {
    console.error('DB 오류:', err);
    res.json({
      is_succeed: false,
      message: 'DB 연결 오류가 발생했습니다.',
      auth_code: ''
    });
  }
};

// validation과 is_valid_token은 그대로 유지 (변경 없음)
exports.validation = (req, res) => {
  const { item } = req.body;

  console.log("auth_code : ", item)
  if(this.is_valid_token(item) == false) {
    res.json({
      is_succeed: false,
      message: '인증에 실패했습니다.',
      auth_code: null
    });
    return;
  }

  res.json({
    is_succeed: true,
    message: '인증에 성공했습니다.',
    auth_code: null
  });
};

exports.is_valid_token = (token) => {
  const expiresAt = tokens[token];

  if (!expiresAt) {
    return false;
  }

  if (Date.now() > expiresAt) {
    delete tokens[token];
    return false;
  }

  return true;
};

// 비밀번호 변경 함수 추가
exports.changePassword = async (req, res) => {
  const { currentPassword, newPassword } = req.body;
  
  try {
    const pool = await sql.connect(option.dbConfig);
    
    // 현재 비밀번호 확인
    const result = await pool.request()
      .query('SELECT TOP 1 password FROM admin_password ORDER BY updated_at DESC');
    
    if (result.recordset.length === 0) {
      return res.json({
        is_succeed: false,
        message: '비밀번호 정보를 찾을 수 없습니다.'
      });
    }
    
    const dbPassword = result.recordset[0].password;
    
    // 현재 비밀번호 검증
    if (currentPassword !== dbPassword) {
      return res.json({
        is_succeed: false,
        message: '현재 비밀번호가 일치하지 않습니다.'
      });
    }
    
    // 새 비밀번호로 업데이트
    await pool.request()
      .input('newPassword', sql.VarChar, newPassword)
      .query('UPDATE admin_password SET password = @newPassword, updated_at = GETDATE()');
    
    res.json({
      is_succeed: true,
      message: '비밀번호가 성공적으로 변경되었습니다.'
    });
    
  } catch (err) {
    console.error('비밀번호 변경 오류:', err);
    res.json({
      is_succeed: false,
      message: 'DB 오류가 발생했습니다.'
    });
  }
};