<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">{{ $t("title.incompleteCustomers") }}</div>
    </div>
  </div>

  <div v-if="isLoading" class="d-flex justify-content-center">
    <LoadingRing />
  </div>
  <div
    v-else-if="!isLoading && data.length == 0"
    class="d-flex justify-content-center card mt-2"
  >
    <div class="d-flex justify-content-center"><NoDataBox /></div>
  </div>
  <div v-else class="card mt-2">
    <el-card v-for="(item, index) in data" :key="index" class="mt-3">
      <div class="d-flex justify-content-between align-items-center">
        <div class="d-flex align-items-center">
          <div>
            <div>{{ item.user?.displayName }}</div>
            <div>{{ item.user?.email }}</div>
          </div>
        </div>

        <div>
          <template v-if="!item.verification.isEmpty">
            {{
              $t("title.verification") +
              " " +
              $t(
                `type.verificationStatus.${item.verification.status}`
              ).toLowerCase()
            }}
          </template>

          <template v-else>
            {{
              $t("title.verification") +
              " " +
              $t(`status.notStarted`).toLowerCase()
            }}
          </template>
          <div>
            <TimeShow
              :date-iso-string="item.createdOn"
              style="font-size: 12px; color: rgb(113, 113, 113)"
            />
          </div>
        </div>
      </div>
    </el-card>
  </div>
</template>
<script setup lang="ts">
import { inject } from "vue";

const data = inject<any>("data");
const isLoading = inject<any>("isLoading");
</script>
