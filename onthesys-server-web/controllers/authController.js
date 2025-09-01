const bcrypt = require('bcrypt');
const crypto = require('crypto');
const option = require('../system/option');
tokens = {}


exports.certification = (req, res) => {
  const { item } = req.body;
console.log("password : " , item)
console.log("truth : " , option.managerPassword)
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
