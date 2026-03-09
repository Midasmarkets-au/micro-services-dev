<template>
  <div class="px-5">
    <div class="">
      <div>
        <el-menu
          :default-active="currentTab"
          class="w-100 mb-5"
          mode="horizontal"
          @select="handleSelect"
        >
          <!--            <el-menu-item :index="VerificationTab.PersonalInfos"-->
          <!--              >Personal Infos</el-menu-item-->
          <!--            >-->

          <el-menu-item :index="VerificationTab.Started"
            >{{ $t("fields.started") }}
          </el-menu-item>
          <el-menu-item :index="VerificationTab.Financial"
            >{{ $t("fields.financial") }}
          </el-menu-item>
          <el-menu-item :index="VerificationTab.Quiz"
            >{{ $t("fields.quiz") }}
          </el-menu-item>
          <el-menu-item :index="VerificationTab.Agreement"
            >{{ $t("fields.agreement") }}
          </el-menu-item>
          <el-menu-item :index="VerificationTab.Info"
            >{{ $t("fields.info") }}
          </el-menu-item>
        </el-menu>
      </div>
      <!-- <a href="#" class="btn btn-primary btn-sm">File Manager</a> -->

      <div class="">
        <UserStart
          v-if="VerificationTab.Started === currentTab"
          :verification-details="verificationDetails"
        />

        <UserInfos
          v-if="VerificationTab.Info === currentTab"
          :verification-details="verificationDetails"
        />
        <UserFinancial
          :verification-details="verificationDetails"
          v-if="VerificationTab.Financial === currentTab"
        />

        <UserQuizs
          :verification-details="verificationDetails"
          v-if="VerificationTab.Quiz === currentTab"
        />

        <UserAgreement
          :verification-details="verificationDetails"
          v-if="VerificationTab.Agreement === currentTab"
        />
        <!--        <UserDocuments-->
        <!--          v-if="VerificationTab.Document === currentTab"-->
        <!--          :verification-details="verificationDetails"-->
        <!--          :verifyOperation="true"-->
        <!--          @update-verification-details="-->
        <!--            loadUserVerifications(verificationDetails.id)-->
        <!--          "-->
        <!--          @user-verification-state-change="handleUserVerificationStatusChange"-->
        <!--          height="625px"-->
        <!--        />-->
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UserDocuments from "@/projects/tenant/modules/users/components/UserDocuments.vue";
import UserFinancial from "@/projects/tenant/modules/users/components/UserFinancial.vue";
import UserStart from "@/projects/tenant/modules/users/components/UserStart.vue";
import UserQuizs from "@/projects/tenant/modules/users/components/UserQuizs.vue";
import UserInfos from "@/projects/tenant/modules/users/components/UserInfos.vue";
import UserAgreement from "@/projects/tenant/modules/users/components/UserAgreement.vue";
import { useStore } from "@/store";
import { onMounted, ref } from "vue";

const enum VerificationTab {
  Started = "Started",
  Info = "Info",
  Financial = "Financial",
  Quiz = "Quiz",
  Agreement = "Agreement",
  Document = "Document",
  PersonalInfos = "Personal",
}

const props = defineProps<{
  verificationDetails: any;
}>();

const store = useStore();
const user = store.state.AuthModule.user;

const currentTab = ref(VerificationTab.Started);
const verificationDetails = ref<any>({});

const handleSelect = (key: VerificationTab, keyPath: string[]) => {
  currentTab.value = key;
};

onMounted(async () => {
  verificationDetails.value = props.verificationDetails;
});
</script>

<style scoped></style>
