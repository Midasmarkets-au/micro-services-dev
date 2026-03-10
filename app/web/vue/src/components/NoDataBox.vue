<template>
  <tr>
    <td colspan="100%">
      <div>
        <div
          class="ant-empty-image mt-6 text-center w-100 d-flex align-items-center justify-content-center"
        >
          <div class="no-data-box">
            <p style="color: #666666">{{ $t("tip.nodata") }}</p>
          </div>
        </div>
      </div>
    </td>
  </tr>
</template>

<script setup lang="ts"></script>

<style scoped>
.no-data-box {
  background-image: url("/images/no_data.png");
  background-size: cover;
  width: 180px;
  height: 147px;
  flex-direction: column;
  justify-content: flex-end;
  display: flex;
}
</style>
