<template>
  <div class="d-flex justify-content-end">
    <button
      class="btn btn-light btn-success btn-md me-3"
      @click="returnTable"
      style="margin-top: -65px; height: 45px"
    >
      Return
    </button>
  </div>

  <div class="card">
    <div class="card-body py-4">
      <div v-if="isLoading" class="row">
        <table>
          <tbody>
            <LoadingRing />
          </tbody>
        </table>
      </div>
      <div v-else class="row">
        <div class="col-4 fixHeightScroll">
          <div class="w-100 card" style="max-width: 880px; margin: auto">
            <!-- -------------------------------------------------Collection of Minimum KYC Information -->
            <!-- -------------------------------------------------Collection of Minimum KYC Information -->
            <div class="card-body">
              <div class="pb-3">
                <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
                  Collection of Minimum KYC Information
                </h2>
              </div>
              <hr />

              <div class="row">
                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.firstName")
                  }}</label>
                  <Field
                    v-model="kycInfos.firstName"
                    tabindex="1"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="firstName"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.firstName"
                      name="firstName"
                      size="large"
                    />
                  </Field>
                </div>

                <!-- Last Name -->
                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.lastName")
                  }}</label>
                  <Field
                    :value="kycInfos.lastName"
                    tabindex="2"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="lastName"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.lastName"
                      name="lastName"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.priorName")
                  }}</label>
                  <Field
                    :value="kycInfos.priorName"
                    tabindex="3"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="priorName"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.priorName"
                      name="priorName"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.birthdate")
                  }}</label>
                  <Field
                    v-model="kycInfos.birthday"
                    tabindex="4"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="birthday"
                    autocomplete="off"
                  >
                    <el-date-picker
                      v-model="kycInfos.birthday"
                      type="date"
                      size="large"
                      value-format="YYYY-MM-DD"
                    />
                  </Field>
                </div>

                <div class="col-lg-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.address")
                  }}</label>
                  <Field
                    v-model="kycInfos.address"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="address"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.address"
                      name="address"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.industry")
                  }}</label>
                  <Field
                    v-model="kycInfos.industry"
                    tabindex="3"
                    class="form-control form-control-lg form-control-solid"
                    name="industry"
                  >
                    <el-select
                      v-model="kycInfos.industry"
                      name="industry"
                      size="large"
                    >
                      <el-option
                        value="government"
                        :label="$t('fields.government')"
                      ></el-option>
                      <el-option
                        value="accounting"
                        :label="$t('fields.accounting')"
                      ></el-option>
                      <el-option
                        value="banking"
                        :label="$t('fields.banking')"
                      ></el-option>
                      <el-option
                        value="finance"
                        :label="$t('fields.finance')"
                      ></el-option>
                      <el-option
                        value="insurance"
                        :label="$t('fields.insurance')"
                      ></el-option>
                      <el-option
                        value="other"
                        :label="$t('fields.other')"
                      ></el-option>
                    </el-select>
                  </Field>
                </div>

                <div class="col-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.position")
                  }}</label>
                  <Field
                    v-model="kycInfos.occupation"
                    tabindex="3"
                    class="form-control form-control-lg form-control-solid"
                    name="position"
                  >
                    <el-select
                      v-model="kycInfos.occupation"
                      name="position"
                      size="large"
                    >
                      <el-option
                        value="director"
                        :label="$t('fields.director')"
                      ></el-option>
                      <el-option
                        value="manager"
                        :label="$t('fields.manager')"
                      ></el-option>
                      <el-option
                        value="entry_level"
                        :label="$t('fields.entryLevel')"
                      ></el-option>
                      <el-option
                        value="other"
                        :label="$t('fields.other')"
                      ></el-option>
                    </el-select>
                  </Field>
                </div>

                <div class="col-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.annualIncome")
                  }}</label>
                  <Field
                    v-model="kycInfos.annualIncome"
                    tabindex="3"
                    class="form-control form-control-lg form-control-solid"
                    name="income"
                  >
                    <el-select
                      v-model="kycInfos.annualIncome"
                      name="income"
                      size="large"
                    >
                      <el-option
                        value="1"
                        :label="$t('fields.range_1')"
                      ></el-option>
                      <el-option
                        value="2"
                        :label="$t('fields.range_1')"
                      ></el-option>
                      <el-option
                        value="3"
                        :label="$t('fields.range_2')"
                      ></el-option>
                      <el-option
                        value="4"
                        :label="$t('fields.range_3')"
                      ></el-option>
                      <el-option
                        value="5"
                        :label="$t('fields.range_4')"
                      ></el-option>
                      <el-option
                        value="6"
                        :label="$t('fields.range_5')"
                      ></el-option>
                    </el-select>
                  </Field>
                </div>

                <div class="col-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.savingsAndInvestments")
                  }}</label>
                  <Field
                    v-model="kycInfos.netWorth"
                    tabindex="3"
                    class="form-control form-control-lg form-control-solid"
                    name="investment"
                  >
                    <el-select
                      v-model="kycInfos.netWorth"
                      name="investment"
                      size="large"
                    >
                      <el-option
                        value="1"
                        :label="$t('fields.range_1')"
                      ></el-option>
                      <el-option
                        value="2"
                        :label="$t('fields.range_2')"
                      ></el-option>
                      <el-option
                        value="3"
                        :label="$t('fields.range_3')"
                      ></el-option>
                      <el-option
                        value="4"
                        :label="$t('fields.range_4')"
                      ></el-option>
                      <el-option
                        value="5"
                        :label="$t('fields.range_5')"
                      ></el-option>
                      <el-option
                        value="6"
                        :label="$t('fields.range_6')"
                      ></el-option>
                    </el-select>
                  </Field>
                </div>
                <div class="col-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.howToFundTrading")
                  }}</label>
                  <Field
                    v-model="kycInfos.sourceOfFunds"
                    tabindex="2"
                    type="text"
                    name="sourceOfFunds"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.sourceOfFunds"
                      tabindex="2"
                      type="text"
                      name="sourceOfFunds"
                      autocomplete="off"
                      size="large"
                    />
                  </Field>
                </div>

                <label
                  class="form-label fs-6 fw-bold text-dark required mt-7"
                  >{{ $t("title.socialMedia") }}</label
                >
                <hr />
                <div
                  class="col-12 mb-3"
                  v-for="(type, index) in kycInfos.socialMediaTypes"
                  :key="index"
                >
                  <div v-if="type.account != ''" class="d-flex">
                    <label
                      class="form-label fs-6 fw-bold text-dark"
                      style="width: 130px"
                      >{{ $t("fields." + type.name) }}</label
                    >
                    <el-input
                      v-model="type.account"
                      tabindex="2"
                      type="text"
                      :name="type.name"
                      autocomplete="off"
                      size="large"
                      disabled
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- ------------------------------------------------- Trading Background -->
          <!-- ------------------------------------------------- Trading Background -->
          <div
            class="w-100 my-5 card verify-card"
            style="max-width: 880px; margin: auto"
          >
            <div class="card-body">
              <div class="pb-3">
                <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
                  {{ $t("title.tradingBackground") }}
                </h2>
              </div>
              <hr />

              <div class="row">
                <div class="col-lg-12 mb-5">
                  <label class="form-label fw-bold text-dark fs-6 required">{{
                    $t("tip.haveYouEverAttendedAnEducationSeminar")
                  }}</label>
                  <div class="row">
                    <div class="col-6 col-lg-2">
                      <Field
                        v-model="kycInfos.bg1"
                        type="radio"
                        class="btn-check"
                        name="bg1"
                        value="0"
                        id="bg1_yes"
                      />
                      <label
                        class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                        for="bg1_yes"
                      >
                        <span class="d-block fw-semobold text-start">
                          <span class="fw-bold d-block fs-4 mb-2">
                            {{ $t("action.yes") }}
                          </span>
                        </span>
                      </label>
                    </div>

                    <!-- --------------------------------------------- -->

                    <div class="col-6 col-lg-2">
                      <Field
                        v-model="kycInfos.bg1"
                        type="radio"
                        class="btn-check"
                        name="bg1"
                        value="1"
                        id="bg1_no"
                      />
                      <label
                        class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                        for="bg1_no"
                      >
                        <span class="d-block fw-semobold text-start">
                          <span class="fw-bold d-block fs-4 mb-2">{{
                            $t("action.no")
                          }}</span>
                        </span>
                      </label>
                    </div>
                  </div>
                </div>
                <div class="col-lg-12 mb-5">
                  <label class="form-label fw-bold text-dark fs-6 required">{{
                    $t("tip.previousExperienceTradingStockBondsCFD")
                  }}</label>
                  <div class="row">
                    <div class="col-6 col-lg-2">
                      <Field
                        v-model="kycInfos.bg2"
                        type="radio"
                        class="btn-check"
                        name="bg2"
                        value="0"
                        id="bg2_yes"
                      />
                      <label
                        class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                        for="bg2_yes"
                      >
                        <span class="d-block fw-semobold text-start">
                          <span class="fw-bold d-block fs-4 mb-2">
                            {{ $t("action.yes") }}
                          </span>
                        </span>
                      </label>
                    </div>

                    <div class="col-6 col-lg-2">
                      <Field
                        v-model="kycInfos.bg2"
                        type="radio"
                        class="btn-check"
                        name="bg2"
                        value="1"
                        id="bg2_no"
                      />
                      <label
                        class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                        for="bg2_no"
                      >
                        <span class="d-block fw-semobold text-start">
                          <span class="fw-bold d-block fs-4 mb-2">{{
                            $t("action.no")
                          }}</span>
                        </span>
                      </label>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- ------------------------------------------------- Trading Experiance -->
          <!-- ------------------------------------------------- Trading Experiance -->
          <div
            class="w-100 card verify-card my-5"
            style="max-width: 880px; margin: auto"
          >
            <div class="card-body">
              <div class="pb-3">
                <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
                  {{ $t("title.relevantTradingExperience") }}
                </h2>
              </div>
              <hr />

              <div class="row">
                <div class="col-lg-12 mb-5">
                  <label class="form-label fw-bold text-dark fs-6 required">
                    {{ $t("tip.verificationFinancialAreYouBcrEmployee") }}
                  </label>
                  <div class="row">
                    <div class="col-6 col-lg-2">
                      <Field
                        v-model="kycInfos.exp1"
                        type="radio"
                        class="btn-check"
                        name="exp1"
                        value="0"
                        id="exp1_true"
                        @click="show_exp1_more = true"
                      />
                      <label
                        class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                        for="exp1_true"
                      >
                        <span class="d-block fw-semobold text-start">
                          <span class="fw-bold d-block fs-4 mb-2">
                            {{ $t("action.yes") }}
                          </span>
                        </span>
                      </label>
                    </div>

                    <div class="col-6 col-lg-2">
                      <Field
                        v-model="kycInfos.exp1"
                        type="radio"
                        class="btn-check"
                        name="exp1"
                        value="1"
                        id="exp1_false"
                        @click="show_exp1_more = false"
                      />
                      <label
                        class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                        for="exp1_false"
                      >
                        <span class="d-block fw-semobold text-start">
                          <span class="fw-bold d-block fs-4 mb-2">{{
                            $t("action.no")
                          }}</span>
                        </span>
                      </label>
                    </div>
                  </div>
                </div>
                <div
                  v-if="show_exp1_more || kycInfos.exp1 == '0'"
                  class="col-lg-12 mb-5"
                >
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.employer")
                  }}</label>
                  <Field
                    v-model="kycInfos.exp1Employer"
                    tabindex="2"
                    type="text"
                    name="exp1Employer"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.exp1Employer"
                      tabindex="2"
                      type="text"
                      name="exp1Employer"
                      size="large"
                      autocomplete="off"
                    />
                  </Field>
                </div>
                <div
                  v-if="show_exp1_more || kycInfos.exp1 == '0'"
                  class="col-lg-12 mb-5"
                >
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.position")
                  }}</label>
                  <Field
                    v-model="kycInfos.exp1Position"
                    tabindex="2"
                    type="text"
                    name="exp1Position"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.exp1Position"
                      tabindex="2"
                      type="text"
                      name="exp1Position"
                      autocomplete="off"
                      size="large"
                    />
                  </Field>
                </div>
                <div
                  v-if="show_exp1_more || kycInfos.exp1 == '0'"
                  class="col-lg-12 mb-5"
                >
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.remuneration")
                  }}</label>
                  <Field
                    v-model="kycInfos.exp1Remuneration"
                    tabindex="2"
                    type="text"
                    name="exp1Remuneration"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.exp1Remuneration"
                      tabindex="2"
                      type="text"
                      name="exp1Remuneration"
                      autocomplete="off"
                      size="large"
                    />
                  </Field>
                </div>
                <div
                  v-for="(v_item, index) in trading_exp"
                  :key="index"
                  class="col-lg-12 mb-5"
                >
                  <label class="form-label fw-bold text-dark fs-6 required">{{
                    v_item.text
                  }}</label>
                  <div class="row">
                    <div class="col-6 col-lg-2">
                      <Field
                        v-model="kycInfos[v_item.id]"
                        type="radio"
                        class="btn-check"
                        :name="v_item.id"
                        value="0"
                        :id="v_item.id + '_true'"
                        @click="v_item.show_more = true"
                      />
                      <label
                        class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                        :for="v_item.id + '_true'"
                      >
                        <span class="d-block fw-semobold text-start">
                          <span class="fw-bold d-block fs-4 mb-2">
                            {{ $t("action.yes") }}</span
                          >
                        </span>
                      </label>
                    </div>

                    <div class="col-6 col-lg-2">
                      <Field
                        v-model="kycInfos[v_item.id]"
                        type="radio"
                        class="btn-check"
                        :name="v_item.id"
                        value="1"
                        :id="v_item.id + '_false'"
                        @click="v_item.show_more = false"
                      />
                      <label
                        class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                        :for="v_item.id + '_false'"
                      >
                        <span class="d-block fw-semobold text-start">
                          <span class="fw-bold d-block fs-4 mb-2">{{
                            $t("action.no")
                          }}</span>
                        </span>
                      </label>
                    </div>
                  </div>

                  <div
                    v-if="v_item.show_more || kycInfos[v_item.id] == '0'"
                    class="col-lg-12 mt-10 ms-3"
                  >
                    <label class="form-label fs-6 fw-bold text-dark required">{{
                      v_item.more_question
                    }}</label>
                    <Field
                      v-model="kycInfos[v_item.id + 'More']"
                      tabindex="2"
                      type="text"
                      :name="v_item.id + 'More'"
                      autocomplete="off"
                    >
                      <el-input
                        v-model="kycInfos[v_item.id + 'More']"
                        tabindex="2"
                        type="text"
                        :name="v_item.id + 'More'"
                        autocomplete="off"
                        size="large"
                      />
                    </Field>
                  </div>
                </div>
              </div>
            </div>
            <!-- <div class="btn btn-primary">{{ item }}</div> -->
          </div>

          <!-- ------------------------------------------------- Sole Trader Only -->
          <!-- ------------------------------------------------- Sole Trader Only -->
          <div class="w-100 card" style="max-width: 880px; margin: auto">
            <div class="card-body">
              <div class="pb-3">
                <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
                  For Sole Trader Only
                </h2>
              </div>
              <hr />

              <div class="row">
                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessName")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessName"
                    tabindex="1"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessName"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessName"
                      name="businessName"
                      size="large"
                    />
                  </Field>
                </div>

                <!-- Last Name -->
                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessNumber")
                  }}</label>
                  <Field
                    :value="kycInfos.businessNumber"
                    tabindex="2"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessNumber"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessNumber"
                      name="businessNumber"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessAddress")
                  }}</label>
                  <Field
                    :value="kycInfos.businessAddress"
                    tabindex="3"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessAddress"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessAddress"
                      name="businessAddress"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessCity")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessCity"
                    tabindex="4"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessCity"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessCity"
                      name="businessCity"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessState")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessState"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessState"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessState"
                      name="businessState"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessZipCode")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessZipCode"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessZipCode"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessZipCode"
                      name="businessZipCode"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessCountry")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessCountry"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessCountry"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessCountry"
                      name="businessCountry"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessCompanyPurposeBetweenUs")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessCompanyPurposeBetweenUs"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessCompanyPurposeBetweenUs"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessCompanyPurposeBetweenUs"
                      name="businessCompanyPurposeBetweenUs"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessServiceRequireReason")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessServiceRequireReason"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessServiceRequireReason"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessServiceRequireReason"
                      name="businessServiceRequireReason"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessIndustry")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessIndustry"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessIndustry"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessIndustry"
                      name="businessIndustry"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessClientIndustry1")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessClientIndustry1"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessClientIndustry1"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessClientIndustry1"
                      name="businessClientIndustry1"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessClientIndustry2")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessClientIndustry2"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessClientIndustry2"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessClientIndustry2"
                      name="businessClientIndustry2"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessClientIndustry3")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessClientIndustry3"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessClientIndustry3"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessClientIndustry3"
                      name="businessClientIndustry3"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-12 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.businessService")
                  }}</label>
                  <Field
                    v-model="kycInfos.businessService"
                    tabindex="8"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="businessService"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.businessService"
                      name="businessService"
                      size="large"
                    />
                  </Field>
                </div>
              </div>
            </div>
          </div>

          <!-- ------------------------------------------------- Verification -->
          <!-- ------------------------------------------------- Verification -->
          <div class="w-100 card my-5" style="max-width: 880px; margin: auto">
            <div class="card-body">
              <div class="pb-3">
                <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
                  Verification
                </h2>
              </div>
              <hr />
              <div class="row">
                <div class="col-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.formId")
                  }}</label>
                  <Field
                    v-model="kycInfos.idType"
                    tabindex="3"
                    class="form-control form-control-lg form-control-solid"
                    name="idType"
                  >
                    <el-select
                      v-model="kycInfos.idType"
                      name="idType"
                      size="large"
                    >
                      <el-option :value="1" label="Government ID"></el-option>
                      <el-option
                        :value="2"
                        label="Driver's License"
                      ></el-option>
                      <el-option :value="3" label="Passport"></el-option>
                    </el-select>
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.doc_number")
                  }}</label>
                  <Field
                    v-model="kycInfos.idNumber"
                    tabindex="1"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="idNumber"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.idNumber"
                      name="idNumber"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.issue_date")
                  }}</label>
                  <Field
                    v-model="kycInfos.idIssuedOn"
                    tabindex="4"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="idIssuedOn"
                    autocomplete="off"
                  >
                    <el-date-picker
                      v-model="kycInfos.idIssuedOn"
                      type="date"
                      size="large"
                      value-format="YYYY-MM-DD"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark">{{
                    $t("fields.expire_date")
                  }}</label>
                  <Field
                    v-model="kycInfos.idExpireOn"
                    tabindex="4"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="idExpireOn"
                    autocomplete="off"
                  >
                    <el-date-picker
                      v-model="kycInfos.idExpireOn"
                      type="date"
                      size="large"
                      value-format="YYYY-MM-DD"
                    />
                  </Field>
                </div>
              </div>
            </div>
          </div>

          <!-- ------------------------------------------------- Admin Use Only -->
          <!-- ------------------------------------------------- Admin Use Only -->
          <div class="w-100 my-5 card" style="max-width: 880px; margin: auto">
            <div class="card-body">
              <div class="pb-3">
                <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
                  Admin Use Only
                </h2>
              </div>
              <hr />
              <div class="row">
                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.adminStaffMember")
                  }}</label>
                  <Field
                    v-model="kycInfos.staffName"
                    tabindex="1"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="staffName"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.staffName"
                      name="idNumber"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.adminStaffPosition")
                  }}</label>
                  <Field
                    v-model="kycInfos.staffPosition"
                    tabindex="1"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="staffPosition"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="kycInfos.staffPosition"
                      name="idNumber"
                      size="large"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.signDate")
                  }}</label>
                  <Field
                    v-model="kycInfos.staffSignedOn"
                    tabindex="4"
                    class="form-control form-control-lg form-control-solid"
                    type="text"
                    name="staffSignedOn"
                    autocomplete="off"
                  >
                    <el-date-picker
                      v-model="kycInfos.staffSignedOn"
                      type="date"
                      size="large"
                      value-format="YYYY-MM-DD"
                    />
                  </Field>
                </div>

                <div class="col-lg-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("action.sign")
                  }}</label>
                  <div>
                    <button
                      class="btn btn-light btn-success btn-sm me-3"
                      style="width: 100%; height: 40px"
                      @click="openSignaturePanel"
                    >
                      {{ $t("action.sign") }}
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- ------------------------------------------------- Operation -->
          <!-- ------------------------------------------------- Operation -->
          <div class="w-100 my-5 card" style="max-width: 880px; margin: auto">
            <div class="card-body d-flex justify-content-center">
              <button
                class="btn btn-light btn-primary btn-lg me-3"
                @click="submitKycForm('update')"
              >
                {{ $t("action.update") }}
              </button>

              <button
                class="btn btn-light btn-warning btn-lg me-3"
                @click="submitKycForm('submit')"
              >
                {{ $t("action.submit") }}
              </button>
            </div>
          </div>
        </div>

        <!-- -------------------------------------------------------------------------------------------------- -->
        <!-- -------------------------------------------------------------------------------------------------- -->
        <!-- -------------------------------------------------------------------------------------------------- -->

        <div class="col-8 fixHeightScroll">
          <div class="w-100 card" style="max-width: 880px; margin: auto">
            <div class="card-body">
              <previewKycForm :kycInfos="kycInfos"></previewKycForm>
            </div>
          </div>
        </div>
      </div>
    </div>

    <SignatureModal
      ref="signatureModalRef"
      @handelSignature="handelSignature"
    />
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { Field } from "vee-validate";
import { ref, onMounted, computed } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import previewKycForm from "./PreviewKycForm.vue";
import SignatureModal from "./SignatureModal.vue";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";

const props = defineProps<{
  selectedPartyId: any;
}>();

const emits = defineEmits<{
  (e: "fetchData", page: number): void;
  (e: "returnTable"): void;
}>();

const signatureModalRef = ref<InstanceType<typeof SignatureModal>>();

const { t } = useI18n();

const isLoading = ref(true);
const kycInfos = ref({} as any);

const show_exp1_more = ref(false);
const trading_exp = computed(() => ({
  exp2: {
    id: "exp2",
    text: t("tip.verificationFinancialHaveBankruptcy"),
    show_more: false,
    more_question: t("tip.verificationFinancialIndicateDischarge"),
  },
  exp3: {
    id: "exp3",
    text: t("tip.verificationFinancialAnyPersonOfCommodityOrRegulatory"),
    show_more: false,
    more_question: t("tip.verificationFinancialProvideExchangeOrAgency"),
  },
  exp4: {
    id: "exp4",
    text: t("tip.verificationFinancialAreYouPep"),
    show_more: false,
    more_question: t("tip.yesProvideDetails"),
  },
  exp5: {
    id: "exp5",
    text: t("tip.verificationFinancialAreYouAssociate"),
    show_more: false,
    more_question: t("tip.yesProvideDetails"),
  },
}));

const loadUserKycInfo = async () => {
  isLoading.value = true;
  try {
    kycInfos.value = await UserService.getPredefinedKycInfos(
      props.selectedPartyId
    );
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const openSignaturePanel = () => {
  signatureModalRef.value?.show();
};

const handelSignature = (_signature) => {
  kycInfos.value.staffSignature = _signature;
  signatureModalRef.value?.hide();
  signatureModalRef.value?.clear();
};

const returnTable = () => {
  emits("returnTable");
};

const submitKycForm = async (_op: string) => {
  try {
    if (_op == "update") {
      await UserService.putKycForm(props.selectedPartyId, kycInfos.value);
      MsgPrompt.success(t("tip.updateSuccess")).then(() => {
        emits("fetchData", 1);
      });
    } else {
      await UserService.signKycForm(props.selectedPartyId, kycInfos.value);
      MsgPrompt.success(t("tip.submitSuccess")).then(() => {
        emits("fetchData", 1);
      });
    }
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(async () => {
  loadUserKycInfo();
});
</script>

<style scoped>
.fixHeightScroll {
  height: 100vh;
  overflow-y: scroll;
}
</style>
