<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.applicationDetails')"
    :is-loading="isLoading"
    :save-title="$t('action.approve')"
    :discard-title="$t('action.reject')"
    :submit="approve"
    :discard="reject"
    :discard-color="`danger`"
    :width="500"
    :before-close="props.beforeClose"
    ><div
      class="d-flex flex-column mb-3"
      v-if="!isLoading"
      style="max-height: 600px; overflow-y: auto"
    >
      <div class="flex-grow-1">
        <div
          class="d-flex d-flex justify-content-around align-items-center mb-6 bg-light-primary rounded p-5"
        >
          <div
            class="symbol symbol-100px symbol-lg-120px symbol-fixed position-relative"
          >
            <AuthImage :imageGuid="applicationDetails.user.avatar" />
            <div
              class="position-absolute translate-middle bottom-0 start-100 mb-6 bg-success rounded-circle border border-4 border-white h-20px w-20px"
            ></div>
          </div>
          <div class="d-flex flex-column">
            <div class="d-flex align-items-center mb-2">
              <a
                href="#"
                class="text-gray-800 text-hover-primary fs-2 fw-bold me-1"
                >{{ applicationDetails.user.name }}</a
              >
              <a href="#">
                <span class="svg-icon svg-icon-1 svg-icon-primary">
                  <inline-svg src="/images/icons/general/gen026.svg" />
                </span>
              </a>

              <a
                href="#"
                class="btn btn-sm btn-light-success fw-bold ms-2 fs-8 py-1 px-3"
                data-bs-toggle="modal"
                data-bs-target="#kt_modal_upgrade_plan"
                >My MDM</a
              >
            </div>

            <div class="d-flex fw-semobold fs-6 mb-4 pe-2">
              <a
                href="#"
                class="d-flex align-items-center text-gray-400 text-hover-primary me-5 mb-2"
              >
                <span class="svg-icon svg-icon-4 me-1">
                  <inline-svg src="/images/icons/communication/com006.svg" />
                </span>
                {{ applicationDetails.user.uid }}
              </a>

              <a
                href="#"
                class="d-flex align-items-center text-gray-400 text-hover-primary mb-2"
              >
                <span class="svg-icon svg-icon-4 me-1">
                  <inline-svg src="/images/icons/communication/com011.svg" />
                </span>
                {{ applicationDetails.user.email }}
              </a>
            </div>
          </div>
        </div>
      </div>
      <div class="separator border-gray-200"></div>

      <div class="mt-6">
        <div class="bg-light-warning rounded p-4 mb-7">
          <div class="row mb-7 fs-4">
            <label class="col-lg-6 fw-semobold text-muted">{{
              $t("fields.partyId")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-dark">
                {{ applicationDetails.partyId }}
              </span>
            </div>
          </div>

          <div
            v-if="
              applicationDetails.supplement.accountUid ||
              applicationDetails.supplement.AccountUid
            "
            class="row mb-7 fs-4"
          >
            <label class="col-lg-6 fw-semobold text-muted">{{
              $t("fields.uid")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-dark">
                {{
                  applicationDetails.supplement.accountUid ??
                  applicationDetails.supplement.AccountUid
                }}
              </span>
            </div>
          </div>

          <div class="row mb-7 fs-4">
            <label class="col-lg-6 fw-semobold text-muted">{{
              $t("fields.createdOn")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-dark">
                <TimeShow :date-iso-string="applicationDetails.createdOn" />
              </span>
            </div>
          </div>

          <div class="row fs-4">
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.type")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-success">
                {{ $t(`type.application.${applicationDetails.type}`) }}
              </span>
            </div>
          </div>
        </div>

        <div
          class="bg-light-danger p-4 rounded mb-7"
          v-if="
            ApplicationType.TradeAccountChangeLeverage ==
            applicationDetails.type
          "
        >
          <div class="row fs-4">
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.newLeverage")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{
                  (applicationDetails.supplement.leverage ??
                    applicationDetails.supplement.Leverage) + ":1"
                }}
              </span>
            </div>
          </div>
        </div>

        <div
          class="bg-light-danger p-4 rounded mb-7"
          v-if="ApplicationType.WholesaleReferral == applicationDetails.type"
        >
          <div class="row fs-4">
            <label class="col-lg-4 fw-bold text-muted">{{
              $t("fields.referral") + " " + $t("fields.email")
            }}</label>

            <div class="col-lg-8">
              <span class="fw-bold text-danger">
                {{ applicationDetails.supplement.email }}
              </span>
            </div>
          </div>

          <div class="row fs-4 mt-4">
            <label class="col-lg-4 fw-bold text-muted">{{
              $t("fields.referral") + " " + $t("fields.account")
            }}</label>

            <div class="col-lg-8">
              <span class="fw-bold text-danger">
                {{ applicationDetails.supplement.accountNumber }}
              </span>
            </div>
          </div>
        </div>

        <!-- <div
          class="bg-light-danger p-4 rounded mb-7"
          v-if="ApplicationType.WholeSaleAccount == applicationDetails.type"
        >
          <div
            v-if="applicationDetails.supplement.request.started.method == 1"
            class="row fs-4"
          >
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.method")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{ $t("tip.wealthThresholdApplication") }}
              </span>
            </div>
          </div>
        </div> -->

        <div
          class="bg-light-danger p-4 rounded mb-7"
          v-if="ApplicationType.WholeSaleAccount == applicationDetails.type"
        >
          <div
            v-if="applicationDetails.supplement.request.started.method == 1"
            class="row fs-4"
          >
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.method")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{ $t("tip.wealthThresholdApplication") }}
              </span>
            </div>
          </div>

          <div
            v-if="applicationDetails.supplement.request.started.method == 2"
            class="row fs-4"
          >
            <!-- method -->
            <div class="row">
              <label class="col-lg-6 fw-bold text-muted">{{
                $t("fields.method")
              }}</label>

              <div class="col-lg-6">
                <span class="fw-bold text-danger">
                  {{ $t("tip.sophisticatedInvestorTest") }}
                </span>
              </div>
            </div>

            <!-- channel -->
            <div class="row mt-7">
              <label class="col-lg-6 fw-bold text-muted">{{
                $t("fields.channel")
              }}</label>

              <div class="col-lg-6">
                <span class="fw-bold text-danger">
                  {{
                    [
                      $t("tip.knowledgeTestTradeHistory"),
                      $t("tip.relevantWorkExperience"),
                      $t("tip.relevantFormalQualifications"),
                    ][applicationDetails.supplement.request.channel.channel - 1]
                  }}
                </span>
              </div>
            </div>

            <!-- supplement -->
            <div
              class="row mt-7"
              v-if="applicationDetails.supplement.request.channel.channel == 1"
            >
              <label class="col-lg-6 fw-bold text-muted">{{
                $t("fields.supplement")
              }}</label>

              <div
                class="col-lg-6 d-flex justify-content-between align-items-center"
              >
                <span class="fw-bold text-danger">
                  <span
                    v-if="
                      applicationDetails.supplement.request.supplement
                        .wrongAns > 1
                    "
                    >{{ $t("tip.testFailType") }}</span
                  >
                  <span v-else>{{ $t("tip.testPassType") }}</span>
                  ({{
                    10 -
                    applicationDetails.supplement.request.supplement.wrongAns
                  }}/10)
                </span>
                <el-button
                  @click="
                    showquizRecord(
                      applicationDetails.supplement.request.supplement.questions
                    )
                  "
                  >View Record</el-button
                >
              </div>
            </div>

            <div
              class="row mt-7"
              v-if="applicationDetails.supplement.request.channel.channel == 2"
            >
              <label class="col-lg-6 fw-bold text-muted mb-3">{{
                $t("fields.supplement")
              }}</label>

              <div class="col-lg-12 fw-bold">
                <div class="mb-3">
                  {{ $t("wholesale.channelTwoQ1") }} <br />
                  <span class="text-danger">
                    Ans:<span class="ms-2">
                      {{
                        applicationDetails.supplement.request.supplement.q1
                      }}</span
                    >
                  </span>
                </div>

                <div class="mb-3">
                  {{ $t("wholesale.channelTwoQ2") }} <br />
                  <span class="text-danger">
                    Ans:<span class="ms-2 text-danger">{{
                      applicationDetails.supplement.request.supplement.q2
                    }}</span>
                  </span>
                </div>

                <div class="mb-3">
                  3.{{ $t("wholesale.channelTwoQ3") }} <br />
                  <span class="text-danger"
                    >Ans:<span class="ms-2 text-danger">{{
                      applicationDetails.supplement.request.supplement.q3
                    }}</span></span
                  >
                </div>

                <div>
                  4.{{ $t("wholesale.channelTwoQ4") }} <br />
                  <span class="text-danger">
                    Ans:<span class="ms-2 text-danger">{{
                      channel2q4Options[
                        applicationDetails.supplement.request.supplement.q4
                      ]
                    }}</span>
                  </span>
                </div>
              </div>
            </div>

            <div
              class="row mt-7"
              v-if="applicationDetails.supplement.request.channel.channel == 3"
            >
              <label class="col-lg-6 fw-bold text-muted mb-3">{{
                $t("fields.supplement")
              }}</label>

              <div class="col-lg-12 fw-bold">
                <div class="mb-3">
                  {{ $t("wholesale.channelThreeQ1") }} <br />
                  <span class="text-danger"
                    >Ans:<span class="ms-2 text-danger">{{
                      applicationDetails.supplement.request.supplement.q1
                    }}</span></span
                  >
                </div>

                <div>
                  {{ $t("wholesale.channelThreeQ2") }} <br />
                  <span class="text-danger">
                    Ans:<span class="ms-2 text-danger">{{
                      applicationDetails.supplement.request.supplement.q2
                    }}</span>
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div
          class="bg-light-danger p-4 rounded mb-7"
          v-if="ApplicationType.Account == applicationDetails.type"
        >
          <div class="row fs-4 mb-5">
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.type")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{
                  $t(
                    `type.account.${applicationDetails.supplement.accountType}`
                  )
                }}
              </span>
            </div>
          </div>

          <div class="row fs-4 mb-5">
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.accountRole")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{ $t(`type.account.${applicationDetails.supplement.role}`) }}
              </span>
            </div>
          </div>

          <div class="row fs-4 mb-5">
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.leverage")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{ applicationDetails.supplement.leverage + ":1" }}
              </span>
            </div>
          </div>

          <div class="row fs-4 mb-5">
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.currency")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{
                  applicationDetails.supplement.currencyId
                    ? $t(
                        `type.currency.${applicationDetails.supplement.currencyId}`
                      )
                    : "***"
                }}
              </span>
            </div>
          </div>

          <div class="row fs-4 mb-5">
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.platform")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{
                  serviceMap[applicationDetails.supplement.platform]
                    .platformName
                }}
              </span>
            </div>
          </div>

          <div class="row fs-4 mb-5">
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.server")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{
                  serviceMap[applicationDetails.supplement.platform].serverName
                }}
              </span>
            </div>
          </div>

          <div class="row fs-4">
            <label class="col-lg-6 fw-bold text-muted">{{
              $t("fields.referralCode")
            }}</label>

            <div class="col-lg-6">
              <span class="fw-bold text-danger">
                {{
                  applicationDetails.supplement.referCode
                    ? applicationDetails.supplement.referCode
                    : "***"
                }}
              </span>
            </div>
          </div>
        </div>

        <!-- <div class="bg-light-info p-4 rounded">
          <div class="row fs-4">
            <label class="col-lg-4 fw-bold text-muted">{{
              $t("fields.notes")
            }}</label>

            <div class="col-lg-8">
              <span class="fw-bold text-success">
                <el-input v-model="formData.reason" type="textarea" />
              </span>
            </div>
          </div>
        </div> -->
      </div>
    </div>

    <QuizRecordModal ref="quizRecordModalRef" />
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick } from "vue";
import AuthImage from "@/components/AuthImage.vue";
import TimeShow from "@/components/TimeShow.vue";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import { ApplicationType } from "@/core/types/ApplicationInfos";
// import { FormRules } from "element-plus";
import SimpleForm from "@/components/SimpleForm.vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import QuizRecordModal from "./modal/ViewQuizRecordModal.vue";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  beforeClose?: () => void;
}>();
const emits = defineEmits<{
  (e: "submit"): void;
}>();
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const isLoading = ref(true);
const isSubmitting = ref(true);
const applicationDetails = ref<any>({});
const { t } = useI18n();

const serviceMap = ref({} as any);
const formData = ref({} as any);

const quizRecordModalRef = ref<InstanceType<typeof QuizRecordModal>>();

const channel2q4Options = ref({
  1: t("wholesale.q4Option1"),
  2: t("wholesale.q4Option2"),
  3: t("wholesale.q4Option3"),
  4: t("wholesale.q4Option4"),
  5: t("wholesale.q4Option5"),
  6: t("wholesale.q4Option6"),
});

const approve = async () => {
  try {
    isSubmitting.value = true;
    await AccountService.approveApplicationsById(
      applicationDetails.value.id,
      formData.value
    );
    MsgPrompt.success().then(() => {
      emits("submit");
      modalRef.value?.hide();
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = false;
  }
};

const reject = async () => {
  try {
    isSubmitting.value = true;
    await AccountService.rejectApplicationsById(
      applicationDetails.value.id,
      formData.value
    );
    MsgPrompt.success().then(() => {
      emits("submit");
      modalRef.value?.hide();
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = false;
  }
};

const showquizRecord = (_QnA: any) => {
  quizRecordModalRef.value?.show(_QnA);
};

defineExpose({
  show: async (_applicationDetails: any) => {
    modalRef.value?.show();
    applicationDetails.value = _applicationDetails;
    await nextTick();
    isLoading.value = false;
  },
  hide: () => modalRef.value?.hide(),
});

onMounted(async () => {
  serviceMap.value = await GlobalService.getServiceMap();
});
</script>

<style scoped></style>
