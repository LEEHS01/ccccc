const bcrypt = require('bcrypt');
const crypto = require('crypto');
const sql = require('mssql');
const option = require('../system/option');
tokens = {}

exports.getPassword = async () => {
  try {
    await sql.connect(option.dbConfig);
    const result = await sql.query(`
      SELECT TOP 1 password FROM admin_password 
      ORDER BY updated_at DESC
    `);
    
    if (result.recordset.length > 0) {
      console.log("DB 비밀번호:", result.recordset[0].password);
      return result.recordset[0].password;
    }
    return option.managerPassword;
  } catch (err) {
    console.error('Password fetch error:', err);
    return option.managerPassword;
  }
};


exports.certification = async (req, res) => {  // ← async 추가
  const { item } = req.body;
  console.log("password : ", item);

  // DB에서 비밀번호 가져오기
  const currentPassword = await this.getPassword();
  console.log("truth : ", currentPassword);

   // 비교 부분 확인
  console.log("비교:", item === currentPassword);  // 추가
  console.log("item 타입:", typeof item);  // 추가
  console.log("currentPassword 타입:", typeof currentPassword);  // 추가

  if (item !== option.managerPassword) {
    return res.json({
      is_succeed: false,
      message: '비밀번호가 일치하지 않습니다.',
      auth_code: ''
    });
  }

  // 인증 성공 → 고유한 토큰 생성
  const rawToken = crypto.randomBytes(32).toString('hex'); 
  const expiresAt = Date.now() + 30 * 60 * 1000; //30분

  tokens[rawToken] = expiresAt;

  res.json({
    is_succeed: true,
    message: '인증에 성공했습니다.',
    auth_code: rawToken
  });
  console.log("응답 전송:", {
  is_succeed: true,
  message: '인증에 성공했습니다.',
  auth_code: rawToken
});
};


// 비밀번호 변경 - changePassword 메서드 내부
exports.changePassword = async (req, res) => {
  // JSON 파싱 추가
  const data = JSON.parse(req.body.item);
  const { oldPassword, newPassword, authCode } = data;
  
  console.log("파싱된 값들:");
  console.log("- oldPassword:", oldPassword);
  console.log("- newPassword:", newPassword);
  console.log("- authCode:", authCode);
  
  // 토큰 검증
  if (!this.is_valid_token(authCode)) {
    return res.json({
      is_succeed: false,
      message: '인증이 만료되었습니다. 다시 로그인해주세요.'
    });
  }
  
  // 현재 비밀번호 확인
  const currentPassword = await this.getPassword();
  console.log("DB에서 가져온 비밀번호:", currentPassword);
  console.log("입력받은 비밀번호:", oldPassword);
  console.log("비교 결과:", currentPassword === oldPassword);
  if (oldPassword !== currentPassword) {
    
    return res.json({
      is_succeed: false,
      message: '현재 비밀번호가 일치하지 않습니다.'
    });
  }
  
  // 새 비밀번호 저장
  try {
    await sql.connect(option.dbConfig);
    
    // admin_password 테이블의 정확한 컬럼명 사용
    await sql.query(`
      UPDATE admin_password 
      SET password = '${newPassword}', updated_at = GETDATE()
    `);
    
    console.log('Password changed successfully to:', newPassword);
    
    res.json({
      is_succeed: true,
      message: '비밀번호가 성공적으로 변경되었습니다.'
    });
    
  } catch (err) {
    console.error('Password change error:', err);
    res.json({
      is_succeed: false,
      message: '비밀번호 변경 중 오류가 발생했습니다.'
    });
  }
};


exports.validation = (req, res) => {
  const { item } = req.body;

console.log("auth_code : " , item)
  if(this.is_valid_token(item) == false)
    {
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

  // 1. 토큰이 존재하는지
  if (!expiresAt) {
    return false;
  }

  // 2. 토큰이 아직 유효한지 (현재 시간보다 만료 시간이 더 나중인지)
  if (Date.now() > expiresAt) {
    // 이미 만료된 토큰이면 삭제해도 됨
    delete tokens[token];
    return false;
  }

  // 3. 유효한 토큰
  return true;
};


// exports.tokens = tokens;
