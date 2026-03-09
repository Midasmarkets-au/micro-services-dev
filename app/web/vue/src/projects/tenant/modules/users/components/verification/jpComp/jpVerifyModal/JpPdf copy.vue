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
          <div class="d-flex gap-6">
            <div>
              <div class="text-center">部店</div>
              <div class="d-flex gap-2 mt-2">
                <div class="checkbox" style="width: 33px; height: 50px" />
                <div class="checkbox" style="width: 33px; height: 50px" />
                <div class="checkbox" style="width: 33px; height: 50px" />
              </div>
            </div>
            <div>
              <div class="text-center">口座番号</div>
              <div class="d-flex gap-2 mt-2">
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
                <div class="d-flex gap-2">
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
            <div class="p-1 ms-1">全ての項目にご記入、ご捺印ください。</div>
          </el-col>
          <el-col :span="6"> </el-col>
          <el-col :span="2" class="bk-border no-br no-bb blue-bg">
            <div class="fw-bold fs-5 p-1 text-center">お申込日</div>
          </el-col>
          <el-col :span="8" class="bk-border no-bb">
            <div class="row h-100 align-items-center" style="width: 90%">
              <div class="col-4 text-end">
                {{ getYear(items.updatedOn) }} 年
              </div>
              <div class="col-4 text-end">
                {{ getMonth(items.updatedOn) }} 月
              </div>
              <div class="col-4 text-end">{{ getDay(items.updatedOn) }} 日</div>
            </div>
          </el-col>
        </el-row>

        <!-- native name -->
        <el-row class="position-relative">
          <el-col :span="2" class="blue-bg bk-border no-border-right">
            <div class="p-1 text-center fw-bold">フリガナ</div>
          </el-col>
          <el-col :span="14" class="bk-border no-br">
            <div class="row ms-1 mt-1">
              <div class="col">セイ {{ items.info.nativeNameLast }}</div>
              <div class="col">メイ {{ items.info.nativeNameFirst }}</div>
            </div>
          </el-col>
          <el-col :span="4" class="bk-border blue-bg no-br">
            <div class="p-1 text-center fw-bold">性 別</div>
          </el-col>
          <el-col :span="4" class="bk-border blue-bg">
            <div class="p-1 text-center fw-bold">お届出印鑑</div>
          </el-col>
        </el-row>

        <!-- Name and gender -->
        <el-row>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-1 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              お名前
            </div>
          </el-col>
          <el-col :span="14">
            <div class="bk-border no-br no-bt h-100">
              <div>（自署）</div>
              <div class="row ms-1">
                <div class="col">姓 {{ items.info.lastName }}</div>
                <div class="col">名 {{ items.info.firstName }}</div>
              </div>
            </div>
          </el-col>
          <el-col :span="4" class="bk-border blue-bg no-br no-bt">
            <div
              class="p-1 d-flex align-items-center justify-content-center gap-4 h-100"
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

        <!-- country -->
        <!-- <el-row>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-1 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              国 籍
            </div>
          </el-col>
          <el-col :span="10" class="bk-border no-br no-bt">
            <div class="ms-2 mt-4 p-1 d-flex align-items-center gap-8 fs-8">
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
            </div>
            <div class="fs-9 m-2 mt-4">
              ※
              証券保管振替機構に対する加入者情報の通知、法令・税制遵守のための取扱以外には使用いたしません。
            </div>
          </el-col>
          <el-col :span="8" class="bk-border no-bt no-br no-bb fs-8">
            <div class="blue-bg bb p-1 text-center fw-bold">世帯主との続柄</div>
            <div class="ms-2 mt-2 p-1 d-flex align-items-center gap-8">
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                本人
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                配偶者
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                子ども
              </div>
            </div>
            <div class="ms-2 mt-1">
              その他の場合は、具体的に続柄を記載してください
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt no-bb text-center fs-8">
            <div>印</div>
          </el-col>
        </el-row> -->

        <!-- birthday -->
        <el-row>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-1 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              生年月日
            </div>
          </el-col>
          <el-col :span="8" class="bk-border no-br no-bt">
            <div class="row h-100 align-items-center" style="width: 90%">
              <div class="col-3 text-end">
                {{ getYear(items.info.birthday) }} 年
              </div>
              <div class="col-3 text-end">
                {{ getMonth(items.info.birthday) }} 月
              </div>
              <div class="col-3 text-end">
                {{ getDay(items.info.birthday) }} 日
              </div>
            </div>
          </el-col>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-1 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              年齢
            </div>
          </el-col>
          <el-col :span="8" class="bk-border no-bt no-br">
            <div class="ms-2 p-1 h-100">
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
              class="p-1 text-center d-flex flex-column align-items-center justify-content-center h-100 fw-bold"
            >
              <div>Email</div>
              <div>アドレス</div>
            </div>
          </el-col>

          <el-col :span="8" class="bk-border no-bt no-br">
            <div class="ms-2 p-1 h-100">
              <div class="d-flex align-items-center h-100">
                {{ items.info.email }}
              </div>
            </div>
          </el-col>

          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-1 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              電話
            </div>
          </el-col>

          <el-col :span="8" class="bk-border no-bt no-br">
            <div class="ms-2 p-1 h-100">
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
              class="p-1 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              ご住所
            </div>
          </el-col>
          <el-col :span="22">
            <div class="bk-border no-bt">
              <div class="d-flex ms-2 align-items-center p-1">
                <div>〒</div>
                <div class="ms-2 mt-1">
                  {{ items.info.postalCode }}
                </div>
              </div>
            </div>

            <div class="bk-border no-bt">
              <div class="ms-1 p-1">
                <div>
                  {{ items.info.region }} - {{ items.info.town }} -
                  {{ items.info.village }}
                </div>
                <div>
                  {{ items.info.street }}- {{ items.info.buildingNumber }}
                </div>
                <div class="blue-text fs-9 mt-2">
                  本人確認書類と同一内容をご記載ください。なお、本人確認書類に記載のない建物棟番号またはマンション名等を追加記載の場合は公共料金領収書等をご提示ください。
                </div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Company Address -->
        <!-- <el-row>
          <el-col :span="2" class="blue-bg bk-border no-br no-bt">
            <div
              class="p-1 text-center d-flex align-items-center justify-content-center h-100 fw-bold"
            >
              通信先
            </div>
          </el-col>
          <el-col :span="22">
            <div class="bk-border no-bt no-bb w-25">
              <div class="d-flex ms-2 align-items-center p-1">
                <div>〒</div>
                <div class="ms-2 mt-1"></div>
              </div>
            </div>
            <div class="bk-border">
              <div class="d-flex ms-1 p-1 align-items-center">
                <div>（フリガナ）</div>
              </div>
            </div>
            <div class="bk-border no-bt">
              <div class="ms-1 p-1">
                <div>都道</div>
                <div>府県</div>
                <div class="blue-text fs-9">
                  本人確認書類と同一内容をご記載ください。なお、本人確認書類に記載のない建物棟番号またはマンション名等を追加記載の場合は公共料金領収書等をご提示ください。
                </div>
              </div>
            </div>
          </el-col>
        </el-row> -->

        <!-- Company Info -->
        <el-row class="mt-3">
          <el-col :span="24" class="bk-border blue-bg">
            <div
              class="p-1 text-center d-flex align-items-center h-100 fw-bold"
            >
              ご本人のお勤め先
            </div>
          </el-col>
        </el-row>

        <el-row>
          <el-col :span="2" class="bk-border no-br no-bt blue-bg">
            <div
              class="p-1 text-center d-flex align-items-center justify-content-center h-100"
            >
              勤務先名称
            </div>
          </el-col>
          <el-col :span="6" class="bk-border no-br no-bt">
            <div class="ms-1 p-1 d-flex align-items-center h-100">
              {{ items.info.companyName }}
            </div>
          </el-col>
          <el-col :span="2" class="bk-border no-br no-bt">
            <div
              class="p-1 text-center d-flex align-items-center justify-content-center h-100"
            >
              所属部署
            </div>
          </el-col>
          <el-col :span="6" class="bk-border no-br no-bt">
            <div class="ms-1 p-1 d-flex align-items-center h-100">
              {{ items.info?.affiliatedDepartment }}
            </div>
          </el-col>
          <el-col :span="2" class="bk-border no-br no-bt">
            <div
              class="p-1 text-center d-flex align-items-center justify-content-center h-100"
            >
              お役職
            </div>
          </el-col>
          <el-col :span="6" class="bk-border no-bt">
            <div class="ms-1 p-1 d-flex align-items-center h-100">
              {{ items.info?.affiliatedPosition }}
            </div>
          </el-col>
        </el-row>

        <!-- Company Address -->
        <!-- <el-row>
          <el-col :span="2" class="bk-border no-br no-bt">
            <div
              class="p-1 text-center d-flex flex-column align-items-center justify-content-center h-100"
            >
              <div>お勤め先</div>
              <div>ご住所</div>
            </div>
          </el-col>
          <el-col :span="22" class="bk-border no-bt">
            <div class="p-1 d-flex align-items-center h-100">
              <div style="width: 150px"></div>
              <div>
                <div>都道</div>
                <div>府県</div>
              </div>
            </div>
          </el-col>
        </el-row> -->

        <!-- Company Phone  -->
        <el-row>
          <el-col :span="2" class="bk-border no-br no-bt blue-bg">
            <div
              class="p-1 text-center d-flex flex-column align-items-center justify-content-center h-100"
            >
              <div>勤務先電話</div>
            </div>
          </el-col>
          <el-col :span="6" class="bk-border no-br no-bt">
            <div class="ms-1 p-1 d-flex align-items-center h-100">
              {{ items.info?.companyPhone }}
            </div>
          </el-col>

          <el-col :span="2" class="bk-border blue-bg no-br no-bt">
            <div
              class="p-1 text-center d-flex flex-column align-items-center justify-content-center h-100 fw-bold"
            >
              <div>ご職業</div>
            </div>
          </el-col>
          <el-col :span="14" class="bk-border no-bt">
            <div class="ms-1 p-1 d-flex align-items-center h-100">
              <div>
                {{ getLabelByValue(items.info.occupation, occupations) }}
              </div>
            </div>
          </el-col>
          <!-- <el-col :span="10" class="bk-border no-bt">
            <div class="p-1 d-flex align-items-center h-100">
              <div>
                下欄の「ご職業コード一覧」から選んで番号をご記入ください。
              </div>
            </div>
          </el-col> -->
        </el-row>

        <!-- Job Selections -->
        <!-- <el-row class="mt-3">
          <el-col :span="2" class="bk-border blue-bg no-br">
            <div
              class="p-1 text-center d-flex flex-column align-items-center justify-content-center h-100 fw-bold"
            >
              <div>ご職業</div>
              <div>コード</div>
            </div>
          </el-col>
          <el-col :span="22" class="bk-border">
            <div class="p-1 ms-1">
              <div class="fs-9">
                ※右上の「ご職業」欄に記入する場合のコード番号です。
              </div>
              <div class="fs-9 row">
                <div v-for="job in jobs" :key="job" class="col-3">
                  {{ job }}
                </div>
              </div>
            </div>
          </el-col>
        </el-row> -->

        <!-- Nationality and Residency -->
        <el-row class="mt-3">
          <el-col :span="2" class="bk-border no-br blue-bg">
            <div
              class="p-1 text-center d-flex flex-column align-items-center justify-content-center h-100"
            >
              <div>国籍情報</div>
            </div>
          </el-col>

          <el-col :span="10" class="bk-border no-br">
            <div class="p-1 pt-2 ms-1 d-flex align-items-center gap-4">
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
              class="p-1 text-center d-flex flex-column align-items-center justify-content-center h-100 fw-bold"
            >
              <div>居住国</div>
            </div>
          </el-col>
          <el-col :span="10" class="bk-border">
            <div class="p-1 pt-2 ms-1 d-flex align-items-center gap-4">
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

        <el-row>
          <el-col :span="2" class="bk-border blue-bg no-br no-bt">
            <div
              class="p-1 text-center d-flex flex-column align-items-center justify-content-center h-100 fw-bold"
            >
              <div>米国の納税義務</div>
            </div>
          </el-col>

          <el-col :span="10" class="bk-border no-bt no-br">
            <div class="p-1 ms-1">
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
            <div class="p-2 ms-1 fs-9">
              ※税法上の米国居住者とは、「グリーンカード（アメリカにおける外国人永住権）の所有者」
              または「一般的には、その年の米国滞在住期間が31日以上でかつその年を含めた過去3
              年間の米国滞在住期間の合計が183日以上の個人」を指します。
            </div>
          </el-col>
        </el-row>
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border no-bt p-1 fw-bold no-br text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>外国PEPSに</div>
            <div>関する申告</div>
          </el-col>
          <el-col :span="21" class="bk-border no-bt p-2 d-flex gap-8">
            <div>
              <div class="d-flex">
                <div>
                  <div>外国政府等において重要な公的位置にある方</div>
                  <div>（過去も含む）及びその親族に該当しますか？</div>
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
                裏面[外国PEPSに関する申告書「ご記入上の注意」]をご確認ください。
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

        <el-row class="mt-2">
          <el-col :span="2" class="bk-border no-br blue-bg">
            <div
              class="p-1 text-center d-flex flex-column align-items-center justify-content-center h-100"
            >
              <div>アカウントタイプ</div>
            </div>
          </el-col>

          <el-col :span="4" class="bk-border no-br">
            <div class="p-1 pt-2 ms-1">
              <div class="d-flex align-items-center">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.citizen == 'jp'"
                />
                スタンダード
              </div>
              <div class="d-flex align-items-center mt-2">
                <input
                  type="checkbox"
                  class="styled-checkbox"
                  :checked="items.info.citizen != 'jp'"
                />
                アルファ
              </div>
            </div>
          </el-col>

          <el-col :span="4" class="bk-border blue-bg">
            <div class="bk-border no-bt no-br no-bl">
              <div
                class="p-1 text-center d-flex flex-column align-items-center justify-content-center"
              >
                <div>ご希望お取引口座</div>
              </div>
            </div>
            <div>
              <div
                class="p-1 text-center d-flex flex-column align-items-center justify-content-center"
              >
                <div>投資家区分</div>
              </div>
            </div>
          </el-col>

          <el-col :span="14" class="bk-border no-bl">
            <div class="bk-border no-bt no-br no-bl">
              <div class="p-1 text-center d-flex align-items-center gap-5">
                <div
                  class="d-flex align-items-center"
                  v-for="item in accountTypesSelection"
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
              <div class="p-1 text-center d-flex align-items-center gap-5">
                <div
                  class="d-flex align-items-center"
                  v-for="item in investorDistinctionSelection"
                  :key="item.value"
                >
                  <input
                    type="checkbox"
                    class="styled-checkbox"
                    :checked="items.financial.investorDistinction == item.value"
                  />
                  {{ item.label }}
                </div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Bank Transfer Account Info -->
        <el-row class="mt-3">
          <el-col :span="24" class="bk-border blue-bg">
            <div class="p-1 ms-2 h-100 d-flex align-items-center gap-4">
              <div class="fw-bold">振込先口座指定</div>
              <!-- <div class="fs-9">
                （注）お振込先の口座名義人は、お申込口座名と同一に限ります。また、、ゆうちょ銀行を選択される場合は、振込用の店名・預金種目・口座番号をご記入ください。
              </div> -->
            </div>
          </el-col>
        </el-row>

        <el-row>
          <el-col :span="8" class="bk-border no-bt no-br">
            <div class="p-1 text-center bb">
              <div>金融機関名</div>
            </div>
            <div class="p-1 text-center">
              {{ items.financial.financialInstiutionName }} -
              {{
                getLabelByValue(
                  items.financial.financialInstiutionType,
                  financialInstiutionTypes
                )
              }}
            </div>
          </el-col>
          <el-col :span="8" class="bk-border no-bt no-br">
            <div class="p-1 text-center bb">
              <div>支店名</div>
            </div>
            <div class="p-1 text-center">
              {{ items.financial.branchName }}
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt no-br">
            <div class="p-1 text-center bb">
              <div>預金の種類</div>
            </div>
            <div class="p-1 text-center">
              {{
                getLabelByValue(
                  items.financial.depositType,
                  depositTypesSelection
                )
              }}
            </div>
          </el-col>
          <el-col :span="4" class="bk-border no-bt">
            <div class="p-1 text-center bb">
              <div>口座番号</div>
            </div>
          </el-col>
        </el-row>
        <el-row class="fs-8">
          ※証券保管振替機構に対する加入者情報の通知、法令・税制遵守のための取扱以外には使用いたしません。
        </el-row>

        <!-- <el-row class="mt-3">
          <el-col :span="24" class="bk-border blue-bg">
            <div class="p-1 ms-2 h-100 d-flex align-items-center gap-4">
              <div class="fw-bold">特定口座開設届・特定口座源泉徴収選択届</div>
            </div>
          </el-col>
        </el-row>

        <el-row>
          <el-col :span="24" class="bk-border no-bt">
            <div class="d-flex gap-10 mt-2 ms-2 mb-3 fs-7">
              <div class="position-relative">
                <div class="d-flex align-items-center blue-text">
                  <input type="checkbox" class="styled-checkbox-lg" />
                  <div>
                    <div>特定口座を</div>
                    <div>開設する</div>
                  </div>
                </div>
                <div class="position-absolute">
                  <div class="d-line position-relative">
                    <div class="position-absolute">
                      <div
                        class="arrow-right"
                        style="width: 86px; margin-left: 1px; margin-top: 73px"
                      ></div>
                      <div
                        class="arrow-right"
                        style="width: 86px; margin-left: 1px; margin-top: 38px"
                      ></div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="blue-text blue-br blue-bl px-4">
                <div>
                  特定口座開設届出書】私は、租税特別措置方第37条の11の3第1項又は第2項及び同法第37条の11の2第1項又は
                </div>
                <div>
                  第2項の規定の適用を受けたいので、租税特別措置法施行令第25条の10の2第5項の規定により、この旨お届けします。
                </div>
                <div>
                  また、上記規定の適用を受けるにあたり、「特定口座に係る上場株式等保管委託」に基づき、上場株式等の保管の委託に係
                </div>
                <div>る口座の設定を申し込みます。</div>
              </div>

              <div class="">
                <div class="d-flex align-items-center blue-text">
                  <input type="checkbox" class="styled-checkbox-lg" />
                  <div>
                    <div>特定口座を</div>
                    <div>開設しない</div>
                  </div>
                </div>
              </div>
            </div>

            <div>
              <div style="margin-left: 120px" class="my-4 fs-8">
                <div class="d-flex">
                  <div class="position-relative">
                    <div class="d-flex align-items-center mb-3">
                      <input type="checkbox" class="styled-checkbox-lg" />
                      <div>
                        <div>源泉徴収を選択する</div>
                      </div>
                    </div>
                    <div
                      class="position-absolute"
                      style="width: 462px; left: 136px; top: 16px"
                    >
                      <div class="arrow-right"></div>
                    </div>
                  </div>
                  <div>
                    <div
                      class="d-flex align-items-center"
                      style="margin-left: 500px"
                    >
                      <input type="checkbox" class="styled-checkbox-lg" />
                      <div>
                        <div>上場株式等の配当等との損益を通算する</div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="d-flex">
                  <div class="position-relative">
                    <div class="d-flex align-items-center">
                      <input type="checkbox" class="styled-checkbox-lg" />
                      <div>
                        <div>源泉徴収を選択しない</div>
                      </div>
                    </div>
                    <div
                      class="position-absolute"
                      style="width: 450px; left: 149px; top: 16px"
                    >
                      <div class="arrow-right"></div>
                    </div>
                  </div>
                  <div>
                    <div
                      class="d-flex align-items-center"
                      style="margin-left: 489px"
                    >
                      <input type="checkbox" class="styled-checkbox-lg" />
                      <div>
                        <div>上場株式等の配当等との損益を通算しない</div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="mt-2 d-flex gap-6">
                  <div>
                    <div>
                      ※当該申込書のご住所と本年1月1日現在の居住都道府県が異なる場合、1月1日現在の都道府県を右にご記入ください。
                    </div>
                    <div>（同一都道府県であればご記入不要です。）</div>
                  </div>
                  <div>
                    <div class="d-flex align-items-center">
                      <div
                        class="checkbox"
                        style="width: 100px; height: 30px"
                      />
                      <div>
                        <div>都道</div>
                        <div>府県</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </el-col>
        </el-row> -->

        <div class="page-break"></div>
        <!-- Page 2 -->
        <el-row class="mt-10">
          <el-col :span="24" class="bk-border blue-bg no-bb">
            <div class="p-1 ms-2 h-100 d-flex align-items-center gap-4">
              <div class="fw-bold">
                日本証券業協会の規則により口座開設に必要な事項となっておりますので必ずご記入ください。（顧客カード）
              </div>
            </div>
          </el-col>
        </el-row>

        <el-row class="blue-bg bk-border no-bt no-bb pt-2">
          <el-col :span="3"></el-col>
          <el-col :span="6">
            <div class="bk-border bg-white blue-text no-bb">
              <div class="p-1 text-center fw-bold fs-7">
                ↓下記番号をご記入ください。
              </div>
            </div>
          </el-col>
        </el-row>
        <!-- Motive for transaction -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br text-center d-flex align-items-center justify-content-center"
            >お取引の動機</el-col
          >
          <el-col
            :span="2"
            class="bk-border no-br p-1 d-flex align-items-center justify-content-center"
          >
            <div class="checkbox-css" style="width: 30px; height: 30px">
              <div
                class="d-flex align-items-center justify-content-center h-100"
              ></div>
            </div>
          </el-col>
          <el-col :span="19" class="bk-border fs-7">
            <div>
              <div class="p-2 mx-2">
                <div class="d-flex gap-4 justify-content-between pb-1">
                  <div>1. 紹介</div>
                  <div>2. ダイレクトメール</div>
                  <div>3. 当社からの訪問</div>
                  <div>4. 当社からの電話</div>
                  <div>5. 折込広告</div>
                  <div>6. 店周ビラ</div>
                </div>
                <div class="d-flex gap-4 justify-content-between">
                  <div>7. ポスター</div>
                  <div>8. セミナー</div>
                  <div>9. 社員の親戚知人</div>
                  <div>10. ホームページ</div>
                  <div>11. 新聞・雑誌</div>
                  <div>12. 来店</div>
                </div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- investment experience -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br no-bt text-center d-flex align-items-center justify-content-center"
            >ご投資の経験</el-col
          >
          <el-col :span="21" class="bk-border p-2 no-bt fs-7">
            <div class="row row-cols-5 mb-1">
              <div class="d-flex align-items-center">
                <div>
                  <div>株式現金</div>
                  <div>取引</div>
                </div>
                <div
                  class="checkbox-css mx-1"
                  style="width: 30px; height: 30px"
                >
                  <div
                    class="d-flex align-items-center justify-content-center h-100"
                  ></div>
                </div>
                <div>年</div>
              </div>
              <div class="d-flex align-items-center">
                <div>公社債</div>
                <div
                  class="checkbox-css mx-1"
                  style="width: 30px; height: 30px"
                >
                  <div
                    class="d-flex align-items-center justify-content-center h-100"
                  ></div>
                </div>
                <div>年</div>
              </div>
              <div class="d-flex align-items-center">
                <div>転換社債</div>
                <div
                  class="checkbox-css mx-1"
                  style="width: 30px; height: 30px"
                >
                  <div
                    class="d-flex align-items-center justify-content-center h-100"
                  ></div>
                </div>
                <div>年</div>
              </div>
              <div class="d-flex align-items-center">
                <div>
                  <div>株式信用</div>
                  <div>取引</div>
                </div>
                <div
                  class="checkbox-css mx-1"
                  style="width: 30px; height: 30px"
                >
                  <div
                    class="d-flex align-items-center justify-content-center h-100"
                  ></div>
                </div>
                <div>年</div>
              </div>
              <div class="d-flex align-items-center">
                <div>ワラント</div>
                <div
                  class="checkbox-css mx-1"
                  style="width: 30px; height: 30px"
                >
                  <div
                    class="d-flex align-items-center justify-content-center h-100"
                  ></div>
                </div>
                <div>年</div>
              </div>
            </div>
            <div class="row row-cols-5">
              <div class="d-flex align-items-center">
                <div>
                  <div>先物OP</div>
                </div>
                <div
                  class="checkbox-css mx-1"
                  style="width: 30px; height: 30px"
                >
                  <div
                    class="d-flex align-items-center justify-content-center h-100"
                  ></div>
                </div>
                <div>年</div>
              </div>
              <div class="d-flex align-items-center">
                <div>
                  貯蓄型
                  <div>取引</div>
                </div>
                <div
                  class="checkbox-css mx-1"
                  style="width: 30px; height: 30px"
                >
                  <div
                    class="d-flex align-items-center justify-content-center h-100"
                  ></div>
                </div>
                <div>年</div>
              </div>
              <div class="d-flex align-items-center">
                <div>投資信託</div>
                <div
                  class="checkbox-css mx-1"
                  style="width: 30px; height: 30px"
                >
                  <div
                    class="d-flex align-items-center justify-content-center h-100"
                  ></div>
                </div>
                <div>年</div>
              </div>
              <div class="d-flex align-items-center">
                <div>
                  <div>外国証券</div>
                </div>
                <div
                  class="checkbox-css mx-1"
                  style="width: 30px; height: 30px"
                >
                  <div
                    class="d-flex align-items-center justify-content-center h-100"
                  ></div>
                </div>
                <div>年</div>
              </div>
              <div class="d-flex align-items-center">
                <div>注）</div>
                <div>
                  <div>ご経験のない場合は、</div>
                  <div>0年とご記入ください。</div>
                </div>
              </div>
            </div>
          </el-col>
        </el-row>
        <!-- original Transaction schedule -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br no-bt text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>当初の</div>
            <div>お取引予定</div>
          </el-col>
          <el-col
            :span="2"
            class="bk-border no-br no-bt p-1 d-flex align-items-center justify-content-center"
          >
            <div class="checkbox-css" style="width: 30px; height: 30px">
              <div
                class="d-flex align-items-center justify-content-center h-100"
              ></div>
            </div>
          </el-col>
          <el-col
            :span="19"
            class="bk-border no-bt fs-7 d-flex align-items-center"
          >
            <div class="p-2 mx-6 w-100">
              <div class="d-flex gap-4 justify-content-between pb-1">
                <div>1. 投資信託</div>
                <div>2. 外国証券</div>
                <div>3. 外国投資信託</div>
                <div>4. 集団投資スキーム等持分取引</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Main funds -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br no-bt text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>主たるご資金</div>
            <div>の性格</div>
          </el-col>
          <el-col
            :span="2"
            class="bk-border no-br no-bt p-1 d-flex align-items-center justify-content-center"
          >
            <div class="checkbox-css" style="width: 30px; height: 30px">
              <div
                class="d-flex align-items-center justify-content-center h-100"
              >
                {{ items.financial.mainFund + 1 }}
              </div>
            </div>
          </el-col>
          <el-col
            :span="19"
            class="bk-border no-bt fs-7 d-flex align-items-center"
          >
            <div class="p-2 mx-6 w-100">
              <div class="d-flex gap-4 justify-content-between pb-1">
                <div>1. 余裕資金</div>
                <div>2. 使途確定金</div>
                <div>3. 借入金</div>
                <div>4. 相続財産</div>
                <div>5. 退職金</div>
                <div>6. 年金・生活費</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Investment policy -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br no-bt text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>ご投資の方針</div>
          </el-col>
          <el-col
            :span="2"
            class="bk-border no-br no-bt p-1 d-flex align-items-center justify-content-center"
          >
            <div class="checkbox-css" style="width: 30px; height: 30px">
              <div
                class="d-flex align-items-center justify-content-center h-100"
              ></div>
            </div>
          </el-col>
          <el-col
            :span="19"
            class="bk-border no-bt fs-7 d-flex align-items-center"
          >
            <div class="p-2 mx-6 w-100">
              <div class="d-flex gap-4 justify-content-between pb-1">
                <div>1. 利回り安定重視</div>
                <div>2. 利回り値上がり益重視</div>
                <div>3. 値上がり益重視</div>
                <div>4. 積極的値上がり益重視</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Asset management period -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br no-bt text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>資産運用期間</div>
          </el-col>
          <el-col
            :span="2"
            class="bk-border no-br no-bt p-1 d-flex align-items-center justify-content-center"
          >
            <div class="checkbox-css" style="width: 30px; height: 30px">
              <div
                class="d-flex align-items-center justify-content-center h-100"
              ></div>
            </div>
          </el-col>
          <el-col
            :span="19"
            class="bk-border no-bt fs-7 d-flex align-items-center"
          >
            <div class="p-2 mx-6 w-100">
              <div class="d-flex gap-4 justify-content-between pb-1">
                <div>1. 長期（3年以上）で運用</div>
                <div>2. 中期（1年～3年）で運用</div>
                <div>3. 短期（1年未満）で運用</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Main source of income -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br no-bt text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>主な収入源</div>
          </el-col>
          <el-col
            :span="2"
            class="bk-border no-br no-bt p-1 d-flex align-items-center justify-content-center"
          >
            <div class="checkbox-css" style="width: 30px; height: 30px">
              <div
                class="d-flex align-items-center justify-content-center h-100"
              >
                {{ items.financial.mainIncomeSource + 1 }}
              </div>
            </div>
          </el-col>
          <el-col
            :span="19"
            class="bk-border no-bt fs-7 d-flex align-items-center"
          >
            <div class="p-2 mx-6 w-100">
              <div class="d-flex gap-4 justify-content-between pb-1">
                <div>1. 事業収入</div>
                <div>2. 不動産収入</div>
                <div>3. 給与収入</div>
                <div>4. 利子・配当収入</div>
                <div>5. 年金</div>
                <div>6. 世帯主の収入</div>
                <div>7. なし</div>
                <div>8. その他</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- financial assets -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br no-bt text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>金融資産</div>
          </el-col>
          <el-col
            :span="2"
            class="bk-border no-br no-bt p-1 d-flex align-items-center justify-content-center"
          >
            <div class="checkbox-css" style="width: 30px; height: 30px">
              <div
                class="d-flex align-items-center justify-content-center h-100"
              ></div>
            </div>
          </el-col>
          <el-col
            :span="19"
            class="bk-border no-bt fs-7 d-flex align-items-center"
          >
            <div class="p-2 mx-6 w-100">
              <div class="row row-cols-4 pb-1">
                <div>1. ～300万円未満</div>
                <div>2. 300～500万円未満</div>
                <div>3. 500～1,000万円未満</div>
                <div>4. 1,000～2,000万円未満</div>
              </div>
              <div class="row row-cols-4">
                <div>5. 2,000～3,000万円未満</div>
                <div>6. 3,000～5,000万円未満</div>
                <div>7. 5,000万円～1億円未満</div>
                <div>8. 1億円以上</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- annual income -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br no-bt text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>年収</div>
          </el-col>
          <el-col
            :span="2"
            class="bk-border no-br no-bt p-1 d-flex align-items-center justify-content-center"
          >
            <div class="checkbox-css" style="width: 30px; height: 30px">
              <div
                class="d-flex align-items-center justify-content-center h-100"
              ></div>
            </div>
          </el-col>
          <el-col
            :span="19"
            class="bk-border no-bt fs-7 d-flex align-items-center"
          >
            <div class="p-2 mx-6 w-100">
              <div class="row row-cols-4 pb-1">
                <div>1. ～300万円未満</div>
                <div>2. 300～500万円未満</div>
                <div>3. 500～1,000万円未満</div>
                <div>4. 1,000～2,000万円未満</div>
              </div>
              <div class="row row-cols-4">
                <div>5. 2,000～3,000万円未満</div>
                <div>6. 3,000～5,000万円未満</div>
                <div>7. 5,000万円～1億円未満</div>
                <div>8. 1億円以上</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Electronic delivery distinction -->
        <el-row>
          <el-col :span="24" class="bk-border mt-4">
            <div class="fw-bold p-2 blue-bg">電子交付区分</div>
            <div class="bt d-flex justify-content-around p-2">
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                <div>電子交付する</div>
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                <div>電子交付しない</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- country of residence -->
        <el-row class="mt-4">
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>居住地国</div>
          </el-col>
          <el-col
            :span="15"
            class="bk-border no-br p-1 d-flex align-items-center gap-8"
          >
            <div class="d-flex align-items-center ms-1">
              <input
                type="checkbox"
                class="checkbox-css"
                style="width: 30px; height: 30px"
                :checked="items.info.citizen == 'jp'"
              />
              <div
                class="d-flex align-items-center justify-content-center h-100"
              ></div>
              <div class="ps-1">日本</div>
            </div>
            <div class="d-flex align-items-center">
              <input
                type="checkbox"
                class="checkbox-css"
                style="width: 30px; height: 30px"
                :checked="items.info.citizen != 'jp'"
              />
              <div class="ps-1">日本以外</div>
            </div>

            <div class="d-flex align-items-center gap-4">
              <input
                type="checkbox"
                class="checkbox-css"
                style="width: 30px; height: 30px"
                :checked="items.info.citizen != 'jp'"
              />
              <div>
                <div>※ 「日本以外」にチェック レ された場合は、別途「特定</div>
                <div class="ms-4">
                  取引を行う者の届出書」をご提出いただきます。
                </div>
              </div>
            </div>
          </el-col>
          <el-col
            :span="3"
            class="blue-bg bk-border p-1 fw-bold no-br text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>個人番号</div>
          </el-col>
          <el-col
            :span="3"
            class="bk-border p-1 fw-bold text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>別紙に記載</div>
          </el-col>
        </el-row>

        <!-- Declaration regarding foreign PEPS -->
        <el-row>
          <el-col
            :span="3"
            class="blue-bg bk-border no-bt p-1 fw-bold no-br text-center d-flex flex-column align-items-center justify-content-center"
          >
            <div>外国PEPSに</div>
            <div>関する申告</div>
          </el-col>
          <el-col :span="21" class="bk-border no-bt p-2 d-flex gap-8">
            <div>
              <div class="d-flex">
                <div>
                  <div>外国政府等において重要な公的位置にある方</div>
                  <div>（過去も含む）及びその親族に該当しますか？</div>
                </div>
                <div class="ms-20">
                  <div class="d-flex align-items-center ms-1">
                    <input
                      type="checkbox"
                      class="checkbox-css"
                      style="width: 30px; height: 30px"
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
                    />
                    <div
                      class="d-flex align-items-center justify-content-center h-100"
                    ></div>
                    <div class="ps-1">はい</div>
                  </div>
                </div>
              </div>
              <div class="mt-2">
                裏面[外国PEPSに関する申告書「ご記入上の注意」]をご確認ください。
              </div>
            </div>

            <div class="ms-6 mt-1">
              <div>「はい」の場合は、具体的にご記入ください。</div>
              <div class="bk-border mt-2" style="height: 40px"></div>
            </div>
          </el-col>
        </el-row>

        <!-- Declaration of designated US persons -->
        <el-row>
          <el-col :span="24" class="bk-border mt-4">
            <div class="p-2 blue-bg">
              <div class="fw-bold">特定米国人の申告</div>
              <div class="fs-7 mt-1">
                外国口座税務コンプライアンス法（FATCA)における特定米国人に該当しますか？
              </div>
            </div>
            <div class="bt row row-cols-2 p-2 pt-4">
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                <div>米国市民（米国籍）</div>
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                <div>米国居住者のうち米国永住権所有者</div>
              </div>
            </div>

            <div class="row row-cols-2 p-2">
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                <div>米国居住者のうち、直近3年間で183日以上米国滞在</div>
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                <div>いずれにも該当しません</div>
              </div>
            </div>
            <div class="bt ps-2">
              ※
              特定米国人に該当する場合、別途「外国口座税務コンプライアンス法（FATCA)に係る申告書」をご提出いただきます。
            </div>
          </el-col>
        </el-row>

        <!-- Insider related -->
        <el-row class="mt-4">
          <el-col
            :span="1"
            class="bk-border blue-bg d-flex flex-column justify-content-center align-items-center fw-bold fs-4 text-center"
          >
            <div>内</div>
            <div>部</div>
            <div>者</div>
            <div>関</div>
            <div>連</div>
          </el-col>
          <el-col :span="23" class="bk-border">
            <el-row class="">
              <el-col :span="8" class="fs-8 text-end br p-4">
                <div>発行会社名：</div></el-col
              >
            </el-row>
            <div class="bt fs-7 ps-1 py-2">
              現在、以下の1～9に該当する方は、「内部者」として登録してください。該当する番号に〇をしてください。
            </div>
            <div class="bt p-2">
              <div class="row row-cols-3 mb-2">
                <div>1. 役員等</div>
                <div>2. 役員の配偶者・同居者</div>
                <div>3. 大株主</div>
              </div>
              <div class="row row-cols-3 mb-2 align-items-center">
                <div>4. 執行役員その他役員に準ずる者</div>
                <div>5. 退任役員（退任後1年以内）</div>
                <div class="d-flex align-items-center">
                  <div class="me-2">6.</div>
                  <div>
                    <div>親会社の役員・退任役員・</div>
                    <div>執行役員・重要事実部職員</div>
                  </div>
                </div>
              </div>
              <div class="row row-cols-3 align-items-center">
                <div class="d-flex align-items-center">
                  <div class="me-2">7.</div>
                  <div>
                    <div>子会社の役員・退任役員・</div>
                    <div>執行役員・重要事実部職員</div>
                  </div>
                </div>
                <div>8. 関連会社</div>
                <div>9. その他（ ）</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Social use column -->
        <div class="dash_line mt-4 w-100"></div>
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
                旅券・乗員手帳
              </div>
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                個人番号カード
              </div>
            </div>
            <div class="row row-cols-3 mt-4">
              <div class="d-flex align-items-center">
                <input type="checkbox" class="styled-checkbox" />
                各種健康保険証
              </div>
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
            <el-row class="bk-border p-2">
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
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { occupations } from "@/core/types/jp/verificationInfo";
import {
  accountTypes,
  accountTypesSelection,
  investorDistinctionSelection,
  financialInstiutionTypes,
  depositTypesSelection,
  accountRolesSelection,
  moneyRange,
  mainIncomeSources,
  transactionMotives,
  mainFunds,
  experienceSelections,
  investmentPurposes,
} from "@/core/types/jp/verificationFinancial";
import { getDataByCode } from "@/core/data/phonesData";
import { isMobile } from "@/core/config/WindowConfig";
import { jpDocs } from "@/core/data/bcrDocs";
const items = inject<any>("verificationDetails");
console.log("items", items.value);

const jobs = ref([
  "01:  上場企業役職員",
  "02:  未上場企業役職員",
  "03:  公務員",
  "04:  民間団体",
  "05:  教職員",
  "06:  弁護士・会計士・税理士",
  "07:  医師",
  "08:  自営業",
  "09:  フリーランス",
  "10:  無職（定年退職）",
  "11:  無職（主婦）",
  "12:  無職",
  "13:  パート・アルバイト・派遣・契約社員",
  "14:  学生",
]);

function getLabelByValue(value, array) {
  const data = array.find((data) => data.value === value);
  return data ? data.label : null;
}

function searchByValue(value, range) {
  for (const key in range) {
    if (range[key].value === value) {
      return range[key].label;
    }
  }
  return null; // If no match found
}

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
