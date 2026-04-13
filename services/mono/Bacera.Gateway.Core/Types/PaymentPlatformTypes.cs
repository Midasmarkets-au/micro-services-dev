namespace Bacera.Gateway;

public enum PaymentPlatformTypes
{
    Undefined = -1,
    System = 1,
    Manual = 2,
    Cash = 10,
    Check = 20,
    Wire = 100,
    Help2Pay = 200,
    Help2PayCNY = 201,
    Poli = 210,
    Ocex = 220,
    PayPal = 230,

    USDT = 240,
    ChipPay = 250, // Otc365
    ExLink = 260,
    /// <summary>
    /// Exlink Global for VND, THB, IDR, INR, MXN, KRW, JPY, BRL, PHP
    /// </summary>
    ExLinkGlobal = 265,  
    GPay = 270,
    BipiPay = 280,
    NPay = 290,
    UnionePayX = 300,
    Long77Pay = 310,
    Long77PayUsdt = 311,
    BigPay = 320,
    UEnjoy = 330,
    UsePay = 340,
    FivePayF2F = 350,
    FivePayVA = 360,
    PaymentAsiaRMB = 370,
    DragonPay = 371,
    Bakong = 372,
    OFAPay = 380,
    Monetix = 390,
    EuPay = 400,
    Pay247 = 410,
    ChinaPay = 420,
    Crypto = 500,
    Buzipay = 650, // Credit card payment via Buzipay
    QrCodeTunnel = 660,
    // ExLink = 330,
    // BankWire = 30,
    // UnionPay = 40,
    // FasaPay = 60,
    //
    // // ReSharper disable once InconsistentNaming
    // USDT = 70,
    // Help2Pay = 80,
    // SystemRebate = 500,
}