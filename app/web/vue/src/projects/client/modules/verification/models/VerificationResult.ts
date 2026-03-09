export interface VerificationResult {
  status: number;
  info: VerificationInfo;
  started: VerificationStarted;
  document: VerificationDocument;
  quiz: Answer[];
  agreement: VerificationAgreement;
  financial: VerificationFinancial;
}

export interface VerificationInfo {
  firstName: string;
  lastName: string;
  priorName: string;
  birthdate: string;
  gender: number;
  citizen: number;
  ccc: number;
  phone: number;
  address: string;
  idFrom: string;
  idNumber: string;
  idOffice: string;
  idIssueDate: string;
  idExpiryDate: string;
}

export interface VerificationStarted {
  currency: string;
  accountType: string;
  leverage: number;
  referral: number;
  questions: {
    q1: boolean;
    q2: boolean;
    q3: boolean;
    q4: boolean;
  };
}

export interface VerificationDocument {
  idFront: string;
  idBack: string;
  addressDocument: string;
}

export interface VerificationAgreement {
  consent_1: boolean;
  consent_2: boolean;
  consent_3: boolean;
}

export interface VerificationFinancial {
  industry: string;
  position: string;
  income: string;
  investment: string;
  fund: string;
  bg1: boolean;
  bg2: boolean;
  bg2_more: string;
  exp1: boolean;
  exp2: boolean;
  exp3: boolean;
  exp4: boolean;
  exp5: boolean;
  exp1_employer: string;
  exp1_position: string;
  exp1_remuneration: string;
  exp2_more: string;
  exp3_more: string;
  exp4_more: string;
  exp5_more: string;
}
export interface Quiz {
  id: number;
  question: string;
  language: string;
  correctAnswer: string;
  answers: Answer[];
}

export interface Answer {
  id: number;
  answer: string;
}
