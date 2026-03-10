<template>
  <div id="print" class="position-relative">
    <!-- Page 1 -->
    <!-- Header -->
    <div>
      <el-row>
        <el-col :span="13">
          <div class="blue-bg p-3" style="font-size: 15px">
            <div class="mb-1">
              店頭外国為替証拠金取引・証券CFD取引・商品CFD取引口座設定申込書
            </div>

            <div class="mb-1">
              兼 口座設定約諾書 兼 顧客カード 兼 外国証券取引口座設定申込書
            </div>
            <div class="mb-1">
              兼 取引時確認記録簿 特定取引を行う者の届出書 兼
              外国PEPSに関する申告書
            </div>
            <div class="mb-1">
              兼 情報共有に関する同意書 兼 印鑑票 兼 申請書
            </div>
          </div>
        </el-col>
        <el-col :span="11" class="ps-10">
          <div class="d-flex justify-content-end gap-6">
            <div>
              <div class="text-center bk-border no-bb" style="width: 96%">
                部店
              </div>
              <div class="d-flex gap-3 mt-2">
                <div class="checkbox" style="width: 33px; height: 50px" />
                <div class="checkbox" style="width: 33px; height: 50px" />
                <div class="checkbox" style="width: 33px; height: 50px" />
              </div>
            </div>
            <div>
              <div class="text-center bk-border no-bb" style="width: 98%">
                口座番号
              </div>
              <div class="d-flex gap-3 mt-2">
                <div class="checkbox" style="width: 33px; height: 50px" />
                <div class="checkbox" style="width: 33px; height: 50px" />
                <div class="checkbox" style="width: 33px; height: 50px" />
                <div class="checkbox" style="width: 33px; height: 50px" />
                <div class="checkbox" style="width: 33px; height: 50px" />
                <div class="checkbox" style="width: 33px; height: 50px" />
              </div>
              <div
                class="d-flex justify-content-end align-items-center mt-5 w-100"
              >
                <div class="me-2">扱者</div>
                <div class="d-flex gap-3">
                  <div class="checkbox" style="width: 33px; height: 50px" />
                  <div class="checkbox" style="width: 33px; height: 50px" />
                  <div class="checkbox" style="width: 33px; height: 50px" />
                </div>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>

      <div class="blue-text">
        <div class="d-flex align-items-center">
          <img
            src="/images/logos/jp-logo-sm.jpg"
            alt="logo"
            style="width: 30px; height: 35px"
          />
          <div>あい証券株式会社 御中</div>
          <div>税務署長 殿</div>
        </div>
        <div class="fs-8 ms-1">
          <div>
            私は、「反社会的勢力でないことの確約に関する同意書」について確約の上、貴社の「約款・規定集」に基づき、総合取引口座を申し込みます。
          </div>
          <div>
            私は、所得税法施行令第343条第3項の適用を受けたいのでこの旨申請します。
          </div>
          <div>
            私は、本申込みに係る情報は、貴社が定める個人情報の利用目的の範囲内で利用すること、及び担当の金融商品仲介業が、私の金融商品取引に関する情報を共有し、相互に提供しあうことに同意します。
          </div>
        </div>
      </div>

      <div class="mt-3">
        <!-- application date -->
        <el-row class="justify-content-between">
          <el-col class="blue-bg fs-5 fw-bold" :span="8">
            <div class="p-3 ms-1">全ての項目にご記入、ご捺印ください。</div>
          </el-col>
          <el-col :span="4"> </el-col>
          <el-col :span="2" class="text-end p-3">（自署）</el-col>
          <el-col :span="2" class="bk-border no-br no-bb blue-bg">
            <div class="fw-bold fs-5 p-3 text-center">お申込日</div>
          </el-col>
          <el-col :span="8" class="bk-border no-bb">
            <div class="row h-100 align-items-center" style="width: 90%">
              <div class="col-4 text-end">年</div>
              <div class="col-4 text-end">月</div>
              <div class="col-4 text-end">日</div>
            </div>
          </el-col>
        </el-row>

        <!-- native name -->
        <el-row class="position-relative">
          <el-col :span="2" class="blue-bg bk-border no-border-right">
            <div class="p-3 text-center fw-bold">フリガナ</div>
          </el-col>
          <el-col :span="14" class="bk-border no-br">
            <div class="row ms-1 mt-1">
              <div class="col">セイ</div>
              <div class="col">メイ</div>
            </div>
          </el-col>
          <el-col :span="4" class="bk-border blue-bg no-br">
            <div class="p-3 text-center fw-bold">性 別</div>
          </el-col>
          <el-col :span="4" class="bk-border blue-bg">
            <div class="p-3 text-center fw-bold">お届出印鑑</div>
          </el-col>
        </el-row>

        <!-- Name and gender -->
        <el-row>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-3 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              お名前
            </div>
          </el-col>
          <el-col :span="14">
            <div class="bk-border no-br no-bt h-100">
              <div>（自署）</div>
              <div class="row ms-1">
                <div class="col">姓</div>
                <div class="col">名</div>
              </div>
            </div>
          </el-col>
          <el-col :span="4" class="bk-border bg-gray no-br no-bt">
            <div
              class="p-3 d-flex align-items-center justify-content-center gap-4 h-100"
            >
              <div class="d-flex align-items-center">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.gender == 0"
                />
                男
              </div>
              <div class="d-flex align-items-center">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.gender == 1"
                />
                女
              </div>
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt no-bb fs-8 text-center pt-1">
            お忘れなくご捺印ください
          </el-col>
        </el-row>

        <!-- birthday -->
        <el-row>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-3 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              生年月日
            </div>
          </el-col>
          <el-col :span="2" class="bk-border no-br no-bt p-3">（自署）</el-col>
          <el-col :span="6" class="bk-border no-br no-bt no-bl">
            <div class="row h-100 align-items-center">
              <div class="col-3 text-end">年</div>
              <div class="col-3 text-end">月</div>
              <div class="col-3 text-end">日</div>
            </div>
          </el-col>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-3 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              年齢
            </div>
          </el-col>
          <el-col :span="8" class="bk-border bg-gray no-bt no-br">
            <div class="ms-2 p-3 h-100">
              <div class="d-flex align-items-center">
                {{ items.info.age }} 歳
              </div>
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt no-bb text-center fs-8">
            <div>印</div>
          </el-col>
        </el-row>

        <!-- email and phone -->
        <el-row>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-3 text-center d-flex flex-column align-items-center justify-content-center h-100 fw-bold"
            >
              <div>Email</div>
              <div>アドレス</div>
            </div>
          </el-col>

          <el-col :span="8" class="bk-border no-bt no-br bg-gray">
            <div class="ms-2 p-3 h-100">
              <div class="d-flex align-items-center h-100">
                {{ items.info.email }}
              </div>
            </div>
          </el-col>

          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-3 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              電話
            </div>
          </el-col>

          <el-col :span="8" class="bk-border no-bt no-br bg-gray">
            <div class="ms-2 p-3 h-100">
              <div class="d-flex align-items-center h-100">
                {{ items.info.phone }}
              </div>
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt text-center fs-8">
            <div
              class="d-flex align-items-end justify-content-center"
              style="height: 70%"
              z
            >
              （シャチハタ不可）
            </div>
          </el-col>
        </el-row>

        <!-- address -->
        <el-row>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-3 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              ご住所
            </div>
          </el-col>
          <el-col :span="22">
            <div class="bk-border no-bt">
              <div class="d-flex ms-2 align-items-center p-3">
                <div>〒</div>
              </div>
            </div>

            <div class="bk-border no-bt">
              <div class="p-3">
                <div style="height: 85px">（自署）</div>

                <div class="blue-text fs-9 mt-2">
                  本人確認書類と同一内容をご記載ください。なお、本人確認書類に記載のない建物棟番号またはマンション名等を追加記載の場合は公共料金領収書等をご提示ください。
                </div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Company Info -->
        <el-row class="mt-8">
          <el-col :span="24" class="bk-border blue-bg">
            <div
              class="p-3 text-center d-flex align-items-center h-100 fw-bold"
            >
              ご本人のお勤め先
            </div>
          </el-col>
        </el-row>

        <el-row>
          <el-col :span="2" class="bk-border no-br no-bt blue-bg">
            <div
              class="p-3 text-center d-flex align-items-center justify-content-center h-100"
            >
              勤務先名称
            </div>
          </el-col>
          <el-col :span="6" class="bk-border no-br no-bt">
            <div class="ms-1 p-3 d-flex align-items-center h-100">
              {{ items.info.companyName }}
            </div>
          </el-col>
          <el-col :span="2" class="bk-border no-br no-bt blue-bg">
            <div
              class="p-3 text-center d-flex align-items-center justify-content-center h-100"
            >
              所属部署
            </div>
          </el-col>
          <el-col :span="6" class="bk-border no-br no-bt">
            <div class="ms-1 p-3 d-flex align-items-center h-100">
              {{ items.info?.affiliatedDepartment }}
            </div>
          </el-col>
          <el-col :span="2" class="bk-border no-br no-bt blue-bg">
            <div
              class="p-3 text-center d-flex align-items-center justify-content-center h-100"
            >
              お役職
            </div>
          </el-col>
          <el-col :span="6" class="bk-border no-bt">
            <div class="ms-1 p-3 d-flex align-items-center h-100">
              {{ items.info?.affiliatedPosition }}
            </div>
          </el-col>
        </el-row>

        <!-- Company Phone  -->
        <el-row>
          <el-col :span="2" class="bk-border no-br no-bt blue-bg">
            <div
              class="p-3 text-center d-flex flex-column align-items-center justify-content-center h-100"
            >
              <div>勤務先電話</div>
            </div>
          </el-col>
          <el-col :span="6" class="bk-border no-br no-bt">
            <div class="ms-1 p-3 d-flex align-items-center h-100">
              {{ items.info?.companyPhone }}
            </div>
          </el-col>

          <el-col :span="2" class="bk-border blue-bg no-br no-bt">
            <div
              class="p-3 text-center d-flex flex-column align-items-center justify-content-center h-100 fw-bold"
            >
              <div>ご職業</div>
            </div>
          </el-col>
          <el-col :span="14" class="bk-border no-bt">
            <div class="ms-1 p-3 d-flex align-items-center h-100">
              <div>
                {{ getLabelByValue(items.info.occupation, jpOccupations) }}
              </div>
            </div>
          </el-col>
          <!-- <el-col :span="10" class="bk-border no-bt">
            <div class="p-3 d-flex align-items-center h-100">
              <div>
                下欄の「ご職業コード一覧」から選んで番号をご記入ください。
              </div>
            </div>
          </el-col> -->
        </el-row>

        <!-- Nationality and Residency -->
        <el-row class="mt-8">
          <el-col :span="2" class="bk-border no-br blue-bg">
            <div
              class="p-3 text-center d-flex flex-column align-items-center justify-content-center h-100"
            >
              <div>国籍情報</div>
            </div>
          </el-col>

          <el-col :span="10" class="bk-border no-br">
            <div class="p-3 pt-2 ms-1 d-flex align-items-center gap-4">
              <div class="d-flex align-items-center">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.citizen == 'jp'"
                />
                日本
              </div>
              <div class="d-flex align-items-center">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.citizen != 'jp'"
                />
                その他
              </div>
              <div class="text-underline ms-1">
                {{ getDataByCode(items.info?.otherCitizen)?.name }}
              </div>
            </div>
            <div class="ms-2 pt-3 pb-2 fs-9">
              ※米国籍を保有されている方の口座開設には対応しておりません。
            </div>
          </el-col>

          <el-col :span="2" class="bk-border blue-bg no-br">
            <div
              class="p-3 text-center d-flex flex-column align-items-center justify-content-center h-100 fw-bold"
            >
              <div>居住国</div>
            </div>
          </el-col>
          <el-col :span="10" class="bk-border">
            <div class="p-3 pt-2 ms-1 d-flex align-items-center gap-4">
              <div class="d-flex align-items-center">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.countryOfResidence == 'jp'"
                />
                日本
              </div>
              <div class="d-flex align-items-center">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.countryOfResidence != 'jp'"
                />
                日本以外
              </div>
            </div>
            <div class="ms-2 pt-3 pb-2 fs-9">
              ※日本国外にお住まいの方の口座開設には対応しておりません。
            </div>
          </el-col>
        </el-row>

        <!-- US Citizen -->
        <el-row>
          <el-col :span="2" class="bk-border blue-bg no-br no-bt">
            <div
              class="p-3 text-center d-flex flex-column align-items-center justify-content-center h-100 fw-bold"
            >
              <div>米国の納税義務</div>
            </div>
          </el-col>

          <el-col :span="10" class="bk-border no-bt no-br">
            <div class="p-3 ms-1">
              <div class="d-flex align-items-center">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.usTax == false"
                />
                私は米国市民（米国籍保有者）又は米国居住者ではありません。
              </div>
              <div class="d-flex align-items-center mt-2">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.usTax == true"
                />
                私は米国市民（米国籍保有者）又は米国居住者です。
              </div>
            </div>
          </el-col>
          <el-col :span="12" class="bk-border no-bl no-bt">
            <div class="p-3 ms-1 fs-9">
              ※税法上の米国居住者とは、「グリーンカード（アメリカにおける外国人永住権）の所有者」
              または「一般的には、その年の米国滞在住期間が31日以上でかつその年を含めた過去3
              年間の米国滞在住期間の合計が183日以上の個人」を指します。
            </div>
          </el-col>
        </el-row>

        <!-- US Tax Information -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border no-bt p-3 fw-bold no-br text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>外国PEPs・内国PEPs・国際機関PEPsに関する申告</div>
          </el-col>
          <el-col :span="21" class="bk-border no-bt p-3 d-flex gap-8">
            <div>
              <div class="d-flex">
                <div>
                  <div>外国政府等外国政府等・内国政府等・国際機関等</div>
                  <div>において重要な公的位置にある方（過去も含む）</div>
                  <div>及びそ の親族に該当しますか？</div>
                </div>
                <div class="ms-20">
                  <div class="d-flex align-items-center ms-1">
                    <input
                      type="checkbox"
                      class="checkbox-css"
                      style="width: 30px; height: 30px"
                      :checked="items.info.usTax == 'no'"
                    />
                    <div
                      class="d-flex align-items-center justify-content-center h-100"
                    ></div>
                    <div class="ps-1">いいえ</div>
                  </div>
                </div>
                <div class="ms-4">
                  <div class="d-flex align-items-center ms-1">
                    <input
                      type="checkbox"
                      class="checkbox-css"
                      style="width: 30px; height: 30px"
                      :checked="items.info.usTax != 'no'"
                    />
                    <div
                      class="d-flex align-items-center justify-content-center h-100"
                    ></div>
                    <div class="ps-1">はい</div>
                  </div>
                </div>
              </div>
              <div class="mt-2">
                ※別紙[外国PEPSに関する申告書「ご記入上の注意」]をご確認ください。
              </div>
            </div>

            <div class="ms-6 mt-1">
              <div>「はい」の場合は、具体的にご記入ください。</div>
              <div class="bk-border mt-2" style="height: 40px">
                <div class="ps-2">{{ items.info.otherPeps }}</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Account Types -->
        <el-row class="mt-8">
          <el-col :span="2" class="bk-border no-br blue-bg">
            <div
              class="p-3 text-center d-flex flex-column align-items-center justify-content-center h-100"
            >
              <div>アカウントタイプ</div>
            </div>
          </el-col>

          <el-col :span="4" class="bk-border no-br">
            <div class="p-3 pt-2 ms-1">
              <div class="d-flex align-items-center">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="
                    items.financial.accountRole == accountRoles.standard
                  "
                />
                スタンダード
              </div>
              <div class="d-flex align-items-center mt-2">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.financial.accountRole == accountRoles.alpha"
                />
                アルファ
              </div>
            </div>
          </el-col>

          <el-col :span="4" class="bk-border blue-bg">
            <div class="bk-border no-bt no-br no-bl">
              <div
                class="p-3 text-center d-flex flex-column align-items-center justify-content-center"
              >
                <div>ご希望お取引口座</div>
              </div>
            </div>
            <div>
              <div
                class="p-3 text-center d-flex flex-column align-items-center justify-content-center"
              >
                <div>投資家区分</div>
              </div>
            </div>
          </el-col>

          <el-col :span="14" class="bk-border no-bl">
            <div class="bk-border no-bt no-br no-bl">
              <div class="p-3 text-center d-flex align-items-center gap-5">
                <div
                  class="d-flex align-items-center"
                  v-for="item in jpAccountTypes"
                  :key="item.value"
                >
                  <input
                    type="checkbox"
                    class="styled-checkbox"
                    :checked="items.financial.accountTypes.includes(item.value)"
                  />
                  {{ item.label }}
                </div>
              </div>
            </div>
            <div>
              <div class="p-3 text-center d-flex align-items-center gap-5">
                <div
                  class="d-flex align-items-center"
                  v-for="item in jpInvestorDistinction"
                  :key="item.value"
                >
                  <input
                    type="checkbox"
                    class="styled-checkbox"
                    :checked="
                      items.financial.investorDistinction.includes(item.value)
                    "
                  />
                  {{ item.label }}
                </div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Bank Transfer Account Info -->
        <el-row class="mt-8">
          <el-col :span="24" class="bk-border blue-bg">
            <div class="p-3 ms-2 h-100 d-flex align-items-center gap-4">
              <div class="fw-bold">振込先口座指定</div>
              <!-- <div class="fs-9">
                （注）お振込先の口座名義人は、お申込口座名と同一に限ります。また、、ゆうちょ銀行を選択される場合は、振込用の店名・預金種目・口座番号をご記入ください。
              </div> -->
            </div>
          </el-col>
        </el-row>

        <el-row>
          <el-col :span="8" class="bk-border no-bt no-br">
            <div class="p-3 text-center bb bg-gray">
              <div>金融機関名</div>
            </div>
            <div class="p-3 text-center">
              {{ items.financial.financialInstiutionName }}
              {{
                getLabelByValue(
                  items.financial.financialInstiutionType,
                  jpFinancialInstitutionTypes
                )
              }}
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt no-br">
            <div class="p-3 text-center bb bg-gray">
              <div>金融機関コード</div>
            </div>
            <div class="p-3 text-center">
              {{ items.financial.financialInstitutionCode }}
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt no-br">
            <div class="p-3 text-center bb bg-gray">
              <div>支店名</div>
            </div>
            <div class="p-3 text-center">
              {{ items.financial.branchName }}
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt no-br">
            <div class="p-3 text-center bb bg-gray">
              <div>支店コード</div>
            </div>
            <div class="p-3 text-center">
              {{ items.financial.branchCode }}
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt">
            <div class="p-3 text-center bb bg-gray">
              <div>預金の種類</div>
            </div>
            <div class="p-3 text-center">
              {{ getLabelByValue(items.financial.depositType, jpDepositTypes) }}
            </div>
          </el-col>
        </el-row>
        <el-row>
          <el-col :span="8" class="bk-border no-bt no-br">
            <div class="p-3 text-center bb bg-gray">
              <div>口座番号</div>
            </div>
            <div class="p-3 text-center">
              {{ items.financial?.accountNumber }}
            </div>
          </el-col>
          <el-col :span="8" class="bk-border no-bt no-br">
            <div class="p-3 text-center bb bg-gray">
              <div>口座名義人(全角)</div>
            </div>
            <div class="p-3 text-center">
              {{ items.financial.accountHolderFullWidth }}
            </div>
          </el-col>

          <el-col :span="8" class="bk-border no-bt">
            <div class="p-3 text-center bb bg-gray">
              <div>口座名義人(全角カタカナ)</div>
            </div>
            <div class="p-3 text-center">
              {{ items.financial.accountHolderFullWidthKatakana }}
            </div>
          </el-col>
        </el-row>

        <div class="page-break"></div>
        <!-- Page 2 -->

        <!-- Financial Info Start -->
        <el-row class="mt-10">
          <el-col :span="24" class="bk-border blue-bg no-bb">
            <div class="p-3 ms-2 h-100 d-flex align-items-center gap-4">
              <div class="fw-bold">顧客カード</div>
            </div>
          </el-col>
        </el-row>

        <!-- Annual Income and Investment -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-3 fw-bold no-br no-bb text-center d-flex align-items-center justify-content-center"
            >ご年収</el-col
          >
          <el-col :span="9" class="bk-border no-br no-bb p-3">
            <div class="p-3 pt-2 ms-1">
              {{
                getItemByValue(items.financial.annualIncome, jpMoneyRange)
                  ?.label
              }}
            </div>
          </el-col>
          <el-col
            :span="3"
            class="blue-bg bk-border p-3 fw-bold no-br no-bb text-center d-flex align-items-center justify-content-center"
            >金融資産</el-col
          >
          <el-col :span="9" class="bk-border no-bb p-3">
            <div class="p-3 pt-2 ms-1">
              {{
                getItemByValue(items.financial.investment, jpMoneyRange)?.label
              }}
            </div>
          </el-col>
        </el-row>

        <!-- Investment Funds and main income source -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-3 fw-bold no-br no-bb text-center d-flex align-items-center justify-content-center"
            >投資可能資金額</el-col
          >
          <el-col :span="9" class="bk-border no-br no-bb p-3">
            <div class="p-3 pt-2 ms-1">
              {{
                getItemByValue(items.financial.investmentFunds, jpMoneyRange)
                  ?.label
              }}
            </div>
          </el-col>
          <el-col
            :span="3"
            class="blue-bg bk-border p-3 fw-bold no-br no-bb text-center d-flex align-items-center justify-content-center"
            >主な収入源</el-col
          >
          <el-col :span="9" class="bk-border no-bb p-3">
            <div class="p-3 pt-2 ms-1">
              {{
                getItemByValue(
                  items.financial.mainIncomeSource,
                  jpMainIncomeSources
                )?.label
              }}
            </div>
          </el-col>
        </el-row>

        <!-- transaction motive and mainfund -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-3 fw-bold no-br text-center d-flex align-items-center justify-content-center"
            >お取引動機</el-col
          >
          <el-col :span="9" class="bk-border no-br p-3">
            <div class="p-3 pt-2 ms-1">
              {{
                getItemByValue(
                  items.financial.transactionMotive,
                  jpTransactionMotives
                )?.label
              }}
            </div>
          </el-col>
          <el-col
            :span="3"
            class="blue-bg bk-border p-3 fw-bold no-br text-center d-flex align-items-center justify-content-center"
            >主たる資金の性格</el-col
          >
          <el-col :span="9" class="bk-border p-3">
            <div class="p-3 pt-2 ms-1">
              {{ getItemByValue(items.financial.mainFund, jpMainFunds)?.label }}
            </div>
          </el-col>
        </el-row>

        <!-- New -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-3 fw-bold no-bt no-br text-center d-flex align-items-center justify-content-center"
            >資産の運用期間</el-col
          >
          <el-col :span="9" class="bk-border no-br no-bt p-3">
            <div class="p-3 pt-2 ms-1">
              {{
                getItemByValue(
                  items.financial.assetManagementPeriod,
                  jpAssetManagementPeriod
                )?.label
              }}
            </div>
          </el-col>
          <el-col
            :span="3"
            class="blue-bg bk-border p-3 fw-bold no-br no-bt text-center d-flex align-items-center justify-content-center"
            >投資目的</el-col
          >
          <el-col :span="9" class="bk-border p-3 no-bt">
            <div class="h-100 ms-1 d-flex flex-wrap gap-4 align-items-center">
              <div
                v-for="(v_item, index) in items.financial.investmentPurpose"
                :key="index"
              >
                {{ getLabelByValue(v_item, jpInvestmentPurposes) }}
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- investment experience -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-3 fw-bold no-br no-bt text-center d-flex align-items-center justify-content-center"
            >ご投資の経験</el-col
          >
          <el-col :span="21" class="bk-border p-3 no-bt fs-7">
            <div class="row mb-1 gap-10">
              <div
                class="col col-3 d-flex align-items-center justify-content-between"
              >
                <div>FX (外国為替証拠金取引)</div>
                <div class="checkbox-css mx-1">
                  <div
                    class="d-flex align-items-center justify-content-center h-100 p-3"
                  >
                    {{
                      getLabelByValue(
                        items.financial.fx,
                        jpExperienceSelections
                      )
                    }}
                  </div>
                </div>
              </div>
              <div
                class="col col-3 d-flex align-items-center justify-content-between"
              >
                <div>株式取引（信用）</div>
                <div class="checkbox-css mx-1">
                  <div
                    class="d-flex align-items-center justify-content-center h-100 p-3"
                  >
                    {{
                      getLabelByValue(
                        items.financial.stockTradingSpot,
                        jpExperienceSelections
                      )
                    }}
                  </div>
                </div>
              </div>

              <div
                class="col col-3 d-flex align-items-center justify-content-between"
              >
                <div>商品先物取引</div>
                <div class="checkbox-css mx-1">
                  <div
                    class="d-flex align-items-center justify-content-center h-100 p-3"
                  >
                    {{
                      getLabelByValue(
                        items.financial.stockTradingCredit,
                        jpExperienceSelections
                      )
                    }}
                  </div>
                </div>
              </div>
            </div>

            <div class="row mb-1 mt-2 gap-10">
              <div
                class="col col-3 d-flex align-items-center justify-content-between"
              >
                <div>株式取引(現物)</div>
                <div class="checkbox-css mx-1">
                  <div
                    class="d-flex align-items-center justify-content-center h-100 p-3"
                  >
                    {{
                      getLabelByValue(
                        items.financial.indexOption,
                        jpExperienceSelections
                      )
                    }}
                  </div>
                </div>
              </div>
              <div
                class="col col-3 d-flex align-items-center justify-content-between"
              >
                <div>指数・オプション</div>
                <div class="checkbox-css mx-1">
                  <div
                    class="d-flex align-items-center justify-content-center h-100 p-3"
                  >
                    {{
                      getLabelByValue(
                        items.financial.commodityFuturesTrading,
                        jpExperienceSelections
                      )
                    }}
                  </div>
                </div>
              </div>

              <div
                class="col col-3 d-flex align-items-center justify-content-between"
              >
                <div>その他デリバティブ取引</div>
                <div class="checkbox-css mx-1">
                  <div
                    class="d-flex align-items-center justify-content-center h-100 p-3"
                  >
                    {{
                      getLabelByValue(
                        items.financial.otherDerivativesTransaction,
                        jpExperienceSelections
                      )
                    }}
                  </div>
                </div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Social use column -->
        <div class="dash_line mt-8 w-100"></div>
        <div class="fs-7 ms-2">
          <div>＜社用使用欄＞</div>
          <div>本人確認記録簿</div>
        </div>

        <el-row class="fs-7 mt-2">
          <el-col :span="10">
            <div class="row row-cols-3">
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                訪問原本提示
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                来社原本提示
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                郵送
              </div>
            </div>
            <div class="row row-cols-3 mt-4">
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                運転免許証
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                個人番号カード
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                各種健康保険証
              </div>
            </div>
            <div class="row row-cols-3 mt-4">
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                在留カード
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                印鑑登録証明書
              </div>
            </div>
            <div class="row mt-4">
              <div class="d-flex align-items-center col-8">
                <input type="checkbox" class="styled-checkbox" />
                住民票の写し又は記載事項全部証明書
              </div>

              <div class="d-flex align-items-center col-4">
                <input type="checkbox" class="styled-checkbox" />
                その他（ ）
              </div>
            </div>
            <div class="d-flex mt-4 justify-content-between me-14">
              <div>確認日：</div>
              <div>年</div>
              <div>月</div>
              <div>時刻</div>
            </div>
            <div class="d-flex mt-4 justify-content-between me-14">
              <div>取引関係文書送付日：</div>
              <div>年</div>
              <div>月</div>
              <div>時刻</div>
            </div>
            <div class="mt-4">確認者：</div>
          </el-col>
          <el-col :span="9">
            <ul>
              <li>到底口座を開設する金融商品取引業者の営業所</li>
              <li>
                上場株式等の譲渡の対価を支払をする金融商品取引業者の営業所
              </li>
              <li>
                <div>
                  上場株式等の配当、公社債の利子、投資信託の収益の分配、国外
                </div>
                <div>発行株式等の配当の支払の取扱者</div>
              </li>
              <li>先物取引の差金等決済を行う先物取引業者の営業所</li>
              <li>
                <div>
                  特定上場株式配当等勘定が設けられた源泉徴収選択口座が開設さ
                </div>
                <div>れている金融商品取引業者等の営業所</div>
              </li>
            </ul>
            <el-row class="bk-border p-3">
              <el-col :span="4">
                <div>名称</div>
                <div>&nbsp;</div>
                <div>所在地</div>
              </el-col>
              <el-col :span="2">
                <div>
                  <img
                    src="/images/logos/jp-logo-sm.jpg"
                    alt="logo"
                    style="width: 15px; height: 20px"
                  />
                </div>
                <div>〒</div>
              </el-col>
              <el-col :span="18">
                <div>あい証券株式会社</div>
                <div>106-6007</div>
                <div>東京都港区六本木1-6-1泉ガーデンタワー7F</div>
              </el-col>
            </el-row>
          </el-col>
          <el-col
            :span="5"
            class="d-flex flex-column justify-content-center align-items-center"
          >
            <div class="text-center mb-1">受付印</div>
            <div>
              <div class="dotted-circle"></div>
            </div>
          </el-col>
        </el-row>
        <div class="dash_line mt-4 w-100"></div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { inject } from "vue";
import { occupations } from "@/core/types/jp/verificationInfo";
import {
  accountRoles,
  accountTypesSelection,
  investorDistinctionSelection,
  financialInstiutionTypes,
  depositTypesSelection,
  moneyRange,
  mainIncomeSources,
  transactionMotives,
  mainFunds,
  experienceSelections,
  investmentPurposes,
  assetManagementPeriodSelection,
} from "@/core/types/jp/verificationFinancial";
import { getDataByCode } from "@/core/data/phonesData";
import { createI18n } from "vue-i18n";
import messages from "@/locales/index";

const items = inject<any>("verificationDetails");

function getLabelByValue(value, array) {
  const data = array.find((data) => data.value === value);
  return data ? data.label : null;
}

const getItemByValue = (value, items) => {
  if (Array.isArray(items)) {
    return (
      items.find((item) => item.value === value) || { label: "", value: "" }
    );
  } else if (typeof items === "object") {
    return (
      Object.values(items).find((item) => item.value === value) || {
        label: "",
        value: "",
      }
    );
  }
  return {
    label: "",
    value: "",
  };
};

const i18n = createI18n({
  legacy: false,
  locale: "jp-jp",
  globalInjection: false,
  messages,
  fallbackLocale: "en-us",
});

const t = i18n.global.t;

function convertToJpLanguage(arrayToTranslate) {
  const originalData = arrayToTranslate.value;
  return originalData.map((item) => ({
    ...item,
    label: t(item.langKey),
  }));
}

function coverToJpLanguageForReactiveObject(objectToTranslate) {
  const originalData = objectToTranslate;
  return originalData.map((item) => ({
    ...item,
    label: t(item.langKey),
  }));
}

const jpMoneyRange = convertToJpLanguage(moneyRange);
const jpAccountTypes = convertToJpLanguage(accountTypesSelection);
const jpInvestorDistinction = coverToJpLanguageForReactiveObject(
  investorDistinctionSelection
);
const jpFinancialInstitutionTypes = convertToJpLanguage(
  financialInstiutionTypes
);

const jpDepositTypes = convertToJpLanguage(depositTypesSelection);
const jpMainIncomeSources = convertToJpLanguage(mainIncomeSources);
const jpTransactionMotives = convertToJpLanguage(transactionMotives);
const jpMainFunds = convertToJpLanguage(mainFunds);
const jpExperienceSelections = convertToJpLanguage(experienceSelections);
const jpInvestmentPurposes = convertToJpLanguage(investmentPurposes);
const jpAssetManagementPeriod = convertToJpLanguage(
  assetManagementPeriodSelection
);

const jpOccupations = convertToJpLanguage(occupations);

function getYear(isoString) {
  return new Date(isoString).getFullYear(); // Extracts the year
}

function getMonth(isoString) {
  return new Date(isoString).getMonth() + 1; // Extracts the month
}

function getDay(isoString) {
  return new Date(isoString).getDate(); // Extracts the day
}

function formatJapanesePhoneNumber(phoneNumber) {
  // Remove any existing hyphens or spaces
  phoneNumber = phoneNumber.replace(/[-\s]/g, "");

  // Check for mobile phone number pattern
  if (/^0[789]0\d{8}$/.test(phoneNumber)) {
    // Format as 090-1234-5678
    return phoneNumber.replace(/(\d{3})(\d{4})(\d{4})/, "$1-$2-$3");
  }
  // Check for landline with 3-digit area code (e.g., Tokyo: 03-1234-5678)
  else if (/^0\d{1,2}\d{8}$/.test(phoneNumber)) {
    // Format as 03-1234-5678 or 045-123-4567 based on area code
    return phoneNumber.replace(/(\d{2,4})(\d{3,4})(\d{4})/, "$1-$2-$3");
  }
  // Default: Return the unformatted phone number if it doesn't match patterns
  return phoneNumber;
}
</script>
<style scoped>
.bg-gray {
  background-color: #dcdcdc;
}
.text-underline {
  text-decoration: underline;
}
.dotted-circle {
  width: 180px; /* Adjust the size as needed */
  height: 180px; /* Make it a perfect circle */
  border: 2px dotted black; /* Dotted border */
  border-radius: 50%; /* Makes the shape circular */
  display: flex;
  justify-content: center; /* Center the text horizontally */
  align-items: center; /* Center the text vertically */
}
.dash_line {
  border-bottom: 1px dashed #000;
}
.vertical-text {
  writing-mode: vertical-rl; /* Makes the text vertical */
  text-orientation: upright; /* Ensures the characters remain upright */
}
.page-break {
  page-break-before: always; /* Forces a new page before this element */
}
.bg-white {
  background-color: #fff;
}
.blue-br {
  border-right: 1px solid #4878a2;
}
.blue-bl {
  border-left: 1px solid #4878a2;
}
.horizon-line {
  width: 100%;
  height: 1px;
  background-color: black;
}
.d-line {
  width: 1px;
  height: 120px;
  background-color: black;
  margin-left: 14px;
  margin-top: -4px;
}

.arrow-right {
  height: 1px;
  background-color: black;
  position: relative;
}

.arrow-right::after {
  content: "";
  position: absolute;
  top: -5px;
  right: -10px;
  border-width: 5px;
  border-style: solid;
  border-color: transparent transparent transparent black;
}
.checkbox {
  border: 1.5px solid #000;
  margin-right: 5px;
  background-color: #fff;
}
.checkbox-css {
  border: 1.5px solid #767676;
  border-radius: 3px;
  box-sizing: border-box;
}
.styled-checkbox-lg {
  width: 30px; /* Increase the width */
  height: 30px; /* Increase the height */
  border: 1.5px solid #000;
  margin-right: 5px;
  background-color: #fff;
}
.styled-checkbox {
  width: 15px; /* Increase the width */
  height: 15px; /* Increase the height */
  border: 1.5px solid #000;
  margin-right: 5px;
  background-color: #fff;
}
.dash_bl {
  border-left: 1px dashed #000;
}
.bt {
  border-top: 1px solid #000;
}
.bb {
  border-bottom: 1px solid #000;
}
.bl {
  border-left: 1px solid #000;
}
.br {
  border-right: 1px solid #000;
}
.no-bt {
  border-top: none !important;
}
.no-bb {
  border-bottom: none !important;
}
.no-bl {
  border-left: none !important;
}
.no-br {
  border-right: none !important;
}
.w-85 {
  width: 85%;
}
.font-10 {
  font-size: 10px;
}
.font-11 {
  font-size: 11px;
}
.blue-bg {
  background-color: #5c9cd5;
  color: #fff;
}
.blue-text {
  color: #4878a2;
}
table {
  border-collapse: collapse;
  width: 100%;
}
td {
  border: 1px solid #000;
}

.bk-border-right {
  border-right: 1.5px solid #020202;
}
.no-border-right {
  border-right: none !important;
}
.bk-border {
  border: 1.5px solid #020202;
  border: collapse;
}
:deep .el-descriptions__cell {
  word-break: normal !important;
  text-wrap: balance !important;
}

:deep tr {
  page-break-inside: avoid; /* Avoid breaking inside table rows */
}

@media (max-width: 768px) {
  :deep .card-body {
    padding: 0rem 0rem;
  }
}

.box-container {
  border: 1px solid #cccccc;
  border-radius: 10px;
  box-sizing: border-box;
}
.bottom {
  border-bottom: 1px solid #cccccc;
}
.left {
  border-right: 1px solid #cccccc;
  padding: 20px;
  letter-spacing: 0.1px;
}
.right {
  text-align: center;
  margin: auto;
}

.pdf {
  color: #000;
  text-decoration: underline;
  cursor: pointer;
}
.pdf:hover {
  color: #409eff;
  text-decoration: underline;
}

:deep .el-form-item__content {
  justify-content: center;
}
</style>
