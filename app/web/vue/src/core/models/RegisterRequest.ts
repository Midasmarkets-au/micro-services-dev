export interface RegisterRequest {
  email: string;
  password: string;
  confirm_url: string;
  ccc: string;
  country_code: string;
  currency: string;
  first_name: string;
  last_name: string;
  phone: string;
  refer_code: string;
  language?: string;
  source_comment?: string;
  site_id?: number;
  tenant_id?: number;
  otp?: string;
}
