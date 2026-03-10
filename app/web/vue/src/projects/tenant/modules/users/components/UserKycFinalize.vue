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
      <div class="row">
        <div v-if="!viewFinalize" class="col-4 fixHeightScroll">
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
                    name="occupation"
                  >
                    <el-select
                      v-model="kycInfos.occupation"
                      name="occupation"
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
                    name="annualIncome"
                  >
                    <el-select
                      v-model="kycInfos.annualIncome"
                      name="annualIncome"
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

                <div class="col-6 mb-5">
                  <label class="form-label fs-6 fw-bold text-dark required">{{
                    $t("fields.savingsAndInvestments")
                  }}</label>
                  <Field
                    v-model="kycInfos.netWorth"
                    tabindex="3"
                    class="form-control form-control-lg form-control-solid"
                    name="netWorth"
                  >
                    <el-select
                      v-model="kycInfos.netWorth"
                      name="netWorth"
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
              <div class="row">
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
                      v-model="kycInfos[v_item.id + '_more']"
                      tabindex="2"
                      type="text"
                      :name="v_item.id + '_more'"
                      autocomplete="off"
                    >
                      <el-input
                        v-model="kycInfos[v_item.id + '_more']"
                        tabindex="2"
                        type="text"
                        :name="v_item.id + '_more'"
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
          <!-- <div class="w-100 card" style="max-width: 880px; margin: auto">
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
          </div> -->

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

          <!-- ------------------------------------------------- Compliance Use Only -->
          <!-- ------------------------------------------------- Compliance Use Only -->
          <div class="w-100 my-5 card" style="max-width: 880px; margin: auto">
            <div class="card-body">
              <div class="pb-3">
                <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
                  Verification Method
                </h2>
              </div>
              <hr />
              <div class="row">
                <div class="col-lg-12 mt-3">
                  <input
                    v-model="kycInfos.electronicOrTraditional"
                    type="radio"
                    id="verifiMethod"
                    name="electronicOrTraditional"
                    value="electronic"
                    @click="kycInfos.traditionalOption = ''"
                  />
                  <label
                    class="form-label fs-5 fw-bold text-dark required mb-3 ms-3"
                    >Electronic Verification
                  </label>
                  <div class="p-5" style="border: 1px solid lightgray">
                    <div class="d-flex">
                      <div>
                        <input
                          v-model="kycInfos.electronicSourceOption"
                          type="radio"
                          id="electronicSourceOption"
                          name="electronicSourceOption"
                          value="labGroup"
                        />
                        <label class="ms-3" for="html">LAB Group</label>
                      </div>
                      <div class="ms-7">
                        <input
                          v-model="kycInfos.electronicSourceOption"
                          type="radio"
                          id="electronicSourceOption"
                          name="electronicSourceOption"
                          value="dataZoo"
                        />
                        <label class="ms-3" for="css">Data Zoo</label>
                      </div>

                      <div class="ms-7">
                        <input
                          v-model="kycInfos.electronicSourceOption"
                          type="radio"
                          id="electronicSourceOption"
                          name="electronicSourceOption"
                          value="refinitiv"
                        />
                        <label class="ms-3" for="css">Refinitiv</label>
                      </div>
                    </div>
                  </div>

                  <div class="p-5 mt-5" style="border: 1px solid lightgray">
                    <div class="d-flex">
                      <input
                        v-model="kycInfos.electronicOption"
                        type="radio"
                        id="electronicOption"
                        name="electronicOption"
                        value="0"
                      />
                      <div class="ms-7">
                        Reliable and independent documentation
                        <br />可靠且独立的文件
                      </div>
                    </div>

                    <div class="d-flex mt-3">
                      <input
                        v-model="kycInfos.electronicOption"
                        type="radio"
                        id="electronicOption"
                        name="electronicOption"
                        value="1"
                      />
                      <div class="ms-7">
                        <div>
                          Reliable and independent electronic data from at least
                          one (1) separate data source <br />来自至少一 (1)
                          个单独数据源的可靠且独立的电子
                        </div>
                      </div>
                    </div>

                    <div class="d-flex mt-3">
                      <input
                        v-model="kycInfos.electronicOption"
                        type="radio"
                        id="electronicOption"
                        name="electronicOption"
                        value="2"
                      />
                      <div class="ms-7">
                        <div>
                          A combination of both of the above <br />上面两者结合
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <div class="col-lg-12 mt-9">
                  <input
                    v-model="kycInfos.electronicOrTraditional"
                    type="radio"
                    id="verifiMethod"
                    name="electronicOrTraditional"
                    value="traditional"
                    @click="
                      (kycInfos.electronicOption = ''),
                        (kycInfos.electronicSourceOption = '')
                    "
                  />
                  <label
                    class="form-label fs-5 fw-bold text-dark required mb-3 ms-3"
                    >Traditional Verification Method</label
                  >
                  <div class="p-5" style="border: 1px solid lightgray">
                    <div class="d-flex">
                      <input
                        v-model="kycInfos.traditionalOption"
                        type="radio"
                        id="traditionalOption"
                        name="traditionalOption"
                        value="traditional1"
                      />
                      <label class="ms-3" for="html"
                        ><strong>Verification Method 1:</strong></label
                      >
                    </div>
                    <div class="ms-7">
                      Origianl or cerified copy of a primary photographic
                      identification document.
                    </div>
                    <div class="d-flex mt-3">
                      <input
                        v-model="kycInfos.traditionalOption"
                        type="radio"
                        id="traditionalOption"
                        name="traditionalOption"
                        value="traditional2"
                      />
                      <label class="ms-3" for="css"
                        ><strong>Verification Method 2:</strong></label
                      >
                    </div>
                    <div class="ms-7">
                      <div>
                        Original or cerified copu of a primary non-photographic
                        identification document; ANS/OR Original or cerified
                        copy of a secondary identification document;
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- ------------------------------------------------- Compliance Note -->
          <!-- ------------------------------------------------- Compliance Note -->
          <div class="w-100 my-5 card" style="max-width: 880px; margin: auto">
            <div class="card-body">
              <div class="pb-3">
                <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
                  Compliance Note
                </h2>
              </div>
              <hr />
              <textarea
                v-model="kycInfos.complianceNote"
                id="w3review"
                name="w3review"
                style="width: 100%; height: 150px; padding: 10px"
              ></textarea>
            </div>
          </div>

          <!-- ------------------------------------------------- Operation -->
          <!-- ------------------------------------------------- Operation -->
          <div class="w-100 my-5 card" style="max-width: 880px; margin: auto">
            <div class="card-body">
              <div class="d-flex justify-content-center">
                <button
                  v-if="kycInfos.complianceSignature == ''"
                  ref="loadSigBtn"
                  class="btn btn-light btn-success btn-lg me-3"
                  @click="complianceDefaultSignature"
                >
                  <span class="indicator-label"> Sign and Preview</span>

                  <span class="indicator-progress">
                    {{ $t("action.pleaseWait") }}
                    <span
                      class="spinner-border spinner-border-sm align-middle ms-2"
                    ></span>
                  </span>
                </button>

                <button
                  v-else
                  class="btn btn-light btn-primary btn-lg me-3"
                  @click="finalizeKycForm"
                >
                  Submit
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- -------------------------------------------------------------------------------------------------- -->
        <!-- -------------------------------------------------------------------------------------------------- -->
        <!-- -------------------------------------------------------------------------------------------------- -->

        <div class="fixHeightScroll" :class="viewFinalize ? 'col-12' : 'col-8'">
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
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";
import { Field } from "vee-validate";
import { ref, onMounted, computed } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import previewKycForm from "./PreviewKycForm.vue";
import SignatureModal from "./SignatureModal.vue";

const store = useStore();

const props = defineProps<{
  selectedPartyId: any;
  viewFinalize: any;
}>();

const emits = defineEmits<{
  (e: "fetchData"): void;
  (e: "returnTable"): void;
}>();

const signatureModalRef = ref<InstanceType<typeof SignatureModal>>();
const loadSigBtn = ref<HTMLButtonElement | null>(null);

const { t } = useI18n();

const isLoading = ref(true);
const kycInfos = ref({} as any);

const trading_exp = computed(() => ({
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

const formatDateToDashFormat = (date) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
};

const loadUserKycInfo = async () => {
  isLoading.value = true;
  loadSigBtn.value?.removeAttribute("data-kt-indicator");

  try {
    // UserService.getKycInfos(props.selectedPartyId);
    kycInfos.value = await UserService.getPredefinedKycInfos(
      props.selectedPartyId
    );
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const complianceDefaultSignature = async () => {
  loadSigBtn.value?.setAttribute("data-kt-indicator", "on");

  try {
    const res = await UserService.getComplianceSignature();
    kycInfos.value.complianceSignature = res;
    kycInfos.value.complianceName = "Victor Ringor";
    kycInfos.value.compliancePosition = "Compliance Officer";
    kycInfos.value.complianceSignedOn = formatDateToDashFormat(new Date());
  } catch (error) {
    MsgPrompt.error(error);
  }
  loadSigBtn.value?.removeAttribute("data-kt-indicator");
};

const handelSignature = (_signature) => {
  kycInfos.value.complianceSignature = _signature;
  signatureModalRef.value?.hide();
  signatureModalRef.value?.clear();
};

const finalizeKycForm = async () => {
  try {
    await UserService.finalizeKycForm(props.selectedPartyId, kycInfos.value);
    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      emits("fetchData");
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const returnTable = () => {
  emits("returnTable");
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
