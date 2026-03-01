import {
  test,
  expect,
} from '@playwright/test';

const API_BASE_URL = process.env.API_BASE_URL ?? 'http://localhost:5164/api';

test.beforeEach(async ({ request }) => {
  const response = await request.delete(`${API_BASE_URL}/forklift/deleteAll`);
  expect(response.ok()).toBeTruthy();
});

test('file upload csv', async ({
  page,
}) => {
  await page.goto('/list');
  const filePath =
    './tests/files/testfile.csv';
  await page.getByRole('link', { name: 'Import Forklift' }).click();
  await page.getByRole('button', { name: 'Choose File' }).click();
  await page
    .getByRole('button', {
      name: 'Choose File',
    })
    .setInputFiles(filePath);
 await page.getByRole('button', { name: 'Upload' }).click();
  await page.waitForLoadState(
    'networkidle',
  );
  await expect(
    page.getByText(
      'File uploaded successfully',
    ),
  ).toBeVisible();
});

