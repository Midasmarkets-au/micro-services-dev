<?php
function getVisitorIP() {
  $ip = '';
  if (getenv('HTTP_CLIENT_IP'))
      $ip = getenv('HTTP_CLIENT_IP');
  else if(getenv('HTTP_X_FORWARDED_FOR'))
      $ip = getenv('HTTP_X_FORWARDED_FOR');
  else if(getenv('HTTP_X_FORWARDED'))
      $ip = getenv('HTTP_X_FORWARDED');
  else if(getenv('HTTP_FORWARDED_FOR'))
      $ip = getenv('HTTP_FORWARDED_FOR');
  else if(getenv('HTTP_FORWARDED'))
      $ip = getenv('HTTP_FORWARDED');
  else if(getenv('REMOTE_ADDR'))
      $ip = getenv('REMOTE_ADDR');
  else
      $ip = 'UNKNOWN';
  return $ip;
}

// 调用函数获取 IP
$visitorIP = getVisitorIP();

$rawData = file_get_contents("php://input");
$data = json_decode($rawData, true);

if (!is_array($data)) {
  echo json_encode(array("error" => "Invalid request"));
  exit;
}

$data["ip"] = getVisitorIP();
$data["host"] = $_SERVER['HTTP_HOST'];

// 使用 cURL 发送 POST 请求
$ch = curl_init("https://thebcrg.com/api/app/result");
curl_setopt($ch, CURLOPT_CUSTOMREQUEST, "POST");
curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($data));
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
curl_setopt($ch, CURLOPT_HTTPHEADER, array(
    'Content-Type: application/json',
    'Accept: application/json'
));

$response = curl_exec($ch);
curl_close($ch);

// 输出响应
echo "ok";